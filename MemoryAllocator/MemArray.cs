using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(MemArrayProxy<>))]
	public unsafe struct MemArray<T> : IIsCreated where T : unmanaged
	{
		public static readonly MemArray<T> Empty = new () { cachedPtr = CachedPtr<T>.Invalid, Length = 0, growFactor = 0, };

		public CachedPtr<T> cachedPtr;
		public ushort growFactor;
		public uint Length { get; private set; }

		public readonly bool IsCreated
		{
			[INLINE(256)] get => cachedPtr.IsValid();
		}

		[INLINE(256)]
		public MemArray(ref Allocator allocator, uint length, ClearOptions clearOptions = ClearOptions.ClearMemory, ushort growFactor = 1)
		{
			if (length == 0u)
			{
				this = Empty;
				return;
			}

			this = default;
			cachedPtr = default;
			var memPtr = allocator.AllocArray(length, out T* ptr);
			cachedPtr = new CachedPtr<T>(in allocator, ptr, memPtr);
			Length = length;
			this.growFactor = growFactor;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				var size = TSize<T>.size;
				allocator.MemClear(memPtr, 0u, length * size);
			}
		}

		[INLINE(256)]
		public MemArray(ref Allocator allocator, uint elementSize, uint length, ClearOptions clearOptions,
			ushort growFactor = 1)
		{
			if (length == 0u)
			{
				this = Empty;
				return;
			}

			this = default;
			cachedPtr = default;
			var memPtr = allocator.Alloc(elementSize * length, out var tPtr);
			cachedPtr = new CachedPtr<T>(in allocator, (T*)tPtr, memPtr);
			Length = length;
			this.growFactor = growFactor;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				Clear(ref allocator);
			}
		}

		[INLINE(256)]
		public MemArray(ref Allocator allocator, in MemArray<T> arr)
		{
			if (arr.Length == 0u)
			{
				this = Empty;
				return;
			}

			cachedPtr = default;
			Length = arr.Length;
			growFactor = arr.growFactor;

			var memPtr = allocator.AllocArray<T>(arr.Length, out var tPtr);
			cachedPtr = new CachedPtr<T>(in allocator, tPtr, memPtr);
			NativeArrayUtils.CopyNoChecks(ref allocator, in arr, 0u, ref this, 0u, arr.Length);
		}

		[INLINE(256)]
		public MemArray(MemPtr memPtr, uint length, ushort growFactor)
		{
			cachedPtr = new CachedPtr<T>(memPtr);
			Length = length;
			this.growFactor = growFactor;
			cachedPtr = default;
		}

		[INLINE(256)]
		public readonly ref U As<U>(in Allocator allocator, uint index) where U : unmanaged
		{
			E.RANGE(index, 0, Length);
			return ref allocator.RefArray<U>(cachedPtr.memPtr, index);
		}

		[INLINE(256)]
		public void ReplaceWith(ref Allocator allocator, in MemArray<T> other)
		{
			if (other.cachedPtr.memPtr == cachedPtr.memPtr)
			{
				return;
			}

			Dispose(ref allocator);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(ref Allocator allocator, in MemArray<T> other)
		{
			if (other.cachedPtr.memPtr == cachedPtr.memPtr) return;
			if (!cachedPtr.memPtr.IsValid() && !other.cachedPtr.memPtr.IsValid())
				return;
			if (cachedPtr.memPtr.IsValid() && !other.cachedPtr.memPtr.IsValid())
			{
				Dispose(ref allocator);
				return;
			}

			if (cachedPtr.memPtr.IsValid() == false)
				this = new MemArray<T>(ref allocator, other.Length);

			NativeArrayUtils.Copy(ref allocator, in other, ref this);
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			if (cachedPtr.memPtr.IsValid())
			{
				allocator.Free(cachedPtr.memPtr);
			}

			this = default;
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
			if (state && IsCreated)
			{
				cachedPtr = new CachedPtr<T>(in allocator, (T*)GetUnsafePtr(in allocator), cachedPtr.memPtr);
			}
			else
			{
				cachedPtr = default;
			}
		}

		[INLINE(256)]
		public void* GetUnsafePtr(in Allocator allocator)
		{
			return cachedPtr.ReadPtr(in allocator);
		}

		[INLINE(256)]
		public MemPtr GetAllocPtr(in Allocator allocator, uint index)
		{
			return allocator.RefArrayPtr<T>(cachedPtr.memPtr, index);
		}

		[INLINE(256)]
		public ref T Read(in Allocator allocator, uint index)
		{
			E.RANGE(index, 0, Length);
			return ref cachedPtr.Read(in allocator, index);
		}

		public ref T this[in Allocator allocator, int index]
		{
			[INLINE(256)]
			get
			{
				E.RANGE(index, 0, Length);
				return ref *((T*)GetUnsafePtr(in allocator) + index);
			}
		}

		public ref T this[in Allocator allocator, uint index]
		{
			[INLINE(256)]
			get
			{
				E.RANGE(index, 0, Length);
				return ref *((T*)GetUnsafePtr(in allocator) + index);
			}
		}

		[INLINE(256)]
		public bool Resize(ref Allocator allocator, uint newLength,
			ClearOptions options = ClearOptions.ClearMemory)
		{
			return Resize(ref allocator, newLength, growFactor, options);
		}

		[INLINE(256)]
		public bool Resize(ref Allocator allocator, uint newLength, ushort growFactor,
			ClearOptions options = ClearOptions.ClearMemory)
		{
			if (IsCreated == false)
			{
				this = new MemArray<T>(ref allocator, newLength, options, growFactor);
				return true;
			}

			if (newLength <= Length)
			{
				return false;
			}

			newLength *= growFactor;

			var prevLength = Length;
			var arrPtr = allocator.ReAllocArray(cachedPtr.memPtr, newLength, out T* ptr);
			cachedPtr = new CachedPtr<T>(in allocator, ptr, arrPtr);
			if (options == ClearOptions.ClearMemory)
			{
				Clear(ref allocator, prevLength, newLength - prevLength);
			}

			Length = newLength;
			return true;
		}

		[INLINE(256)]
		public bool Resize(ref Allocator allocator, uint elementSize, uint newLength, ClearOptions options = ClearOptions.ClearMemory, ushort growFactor = 1)
		{
			if (IsCreated == false)
			{
				this = new MemArray<T>(ref allocator, newLength, options, growFactor);
				return true;
			}

			if (newLength <= Length)
			{
				return false;
			}

			newLength *= this.growFactor;

			var prevLength = Length;
			var arrPtr = allocator.Alloc(elementSize * newLength, out var tPtr);
			cachedPtr = new CachedPtr<T>(in allocator, (T*)tPtr, arrPtr);
			if (options == ClearOptions.ClearMemory)
			{
				Clear(ref allocator, prevLength, newLength - prevLength);
			}

			Length = newLength;
			return true;
		}

		[INLINE(256)]
		public void Clear(ref Allocator allocator)
		{
			Clear(ref allocator, 0u, Length);
		}

		[INLINE(256)]
		public void Clear(ref Allocator allocator, uint index, uint length)
		{
			E.IS_CREATED(this);

			var size = TSize<T>.size;
			allocator.MemClear(cachedPtr.memPtr, index * size, length * size);
		}

		[INLINE(256)]
		public bool Contains<U>(in Allocator allocator, U obj) where U : unmanaged, System.IEquatable<T>
		{
			E.IS_CREATED(this);
			var ptr = (T*)GetUnsafePtr(in allocator);
			for (uint i = 0, cnt = Length; i < cnt; ++i)
			{
				if (obj.Equals(*(ptr + i)))
				{
					return true;
				}
			}

			return false;
		}

		public uint GetReservedSizeInBytes()
		{
			return Length * (uint)sizeof(T);
		}
	}
}
