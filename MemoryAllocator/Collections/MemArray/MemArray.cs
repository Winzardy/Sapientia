using System.Diagnostics;
using System.Runtime.InteropServices;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(MemArrayProxy<>))]
	public unsafe struct MemArray : IIsCreated
	{
		public static readonly MemArray Empty = new () { ptr = Ptr.Invalid, Length = 0, growFactor = 0, };

		public Ptr ptr;
		public ushort growFactor;
		public uint Length { get; private set; }
		public uint ElementSize { get; private set; }

		public readonly bool IsCreated
		{
			[INLINE(256)] get => ptr.IsValid();
		}

		[INLINE(256)]
		public ref Allocator GetAllocator()
		{
			return ref ptr.GetAllocator();
		}

		[INLINE(256)]
		public MemArray(uint elementSize, uint length, ClearOptions clearOptions = ClearOptions.ClearMemory, ushort growFactor = 1) : this(ref AllocatorManager.CurrentAllocator, elementSize, length, clearOptions, growFactor)
		{

		}

		[INLINE(256)]
		public MemArray(ref Allocator allocator, uint elementSize, uint length, ClearOptions clearOptions = ClearOptions.ClearMemory, ushort growFactor = 1)
		{
			if (length == 0u)
			{
				this = Empty;
				return;
			}

			this = default;
			ptr = default;
			var memPtr = allocator.AllocArray(elementSize, length, out var tPtr);
			ptr = new Ptr(in allocator, tPtr, memPtr);
			ElementSize = elementSize;
			Length = length;
			this.growFactor = growFactor;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				Clear(allocator);
			}
		}

		[INLINE(256)]
		public MemArray(in MemArray arr) : this(ref AllocatorManager.CurrentAllocator, arr)
		{
		}

		[INLINE(256)]
		public MemArray(ref Allocator allocator, in MemArray arr)
		{
			if (arr.Length == 0u)
			{
				this = Empty;
				return;
			}

			ptr = default;
			Length = arr.Length;
			ElementSize = arr.ElementSize;
			growFactor = arr.growFactor;

			var memPtr = allocator.AllocArray(arr.ElementSize, arr.Length, out var tPtr);
			ptr = new Ptr(in allocator, tPtr, memPtr);
			MemArrayExt.CopyNoChecks(ref allocator, in arr, 0u, ref this, 0u, arr.Length);
		}

		[INLINE(256)]
		public MemArray(MemPtr memPtr, uint elementSize, uint length, ushort growFactor)
		{
			ptr = new Ptr(memPtr);
			Length = length;
			ElementSize = elementSize;
			this.growFactor = growFactor;
			ptr = default;
		}

		[INLINE(256)]
		public void ReplaceWith(ref Allocator allocator, in MemArray other)
		{
			if (other.ptr.memPtr == ptr.memPtr)
			{
				return;
			}

			Dispose(ref allocator);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(ref Allocator allocator, in MemArray other)
		{
			if (other.ptr.memPtr == ptr.memPtr) return;
			if (!ptr.memPtr.IsValid() && !other.ptr.memPtr.IsValid())
				return;
			if (ptr.memPtr.IsValid() && !other.ptr.memPtr.IsValid())
			{
				Dispose(ref allocator);
				return;
			}

			if (ptr.memPtr.IsValid() == false)
				this = new MemArray(ref allocator, other.ElementSize, other.Length, ClearOptions.ClearMemory);

			MemArrayExt.Copy(ref allocator, in other, ref this);
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			if (ptr.memPtr.IsValid())
			{
				allocator.Free(ptr.memPtr);
			}

			this = default;
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
			if (state && IsCreated)
			{
				ptr = new Ptr(in allocator, GetPtr(in allocator), ptr.memPtr);
			}
			else
			{
				ptr = default;
			}
		}

		[INLINE(256)]
		public void* GetPtr(in Allocator allocator)
		{
			return ptr.GetPtr(in allocator);
		}

		[INLINE(256)]
		public void* GetPtr(Allocator* allocator)
		{
			return ptr.GetPtr(allocator);
		}

		[INLINE(256)]
		public void* GetPtr()
		{
			return ptr.GetPtr();
		}

		[INLINE(256)]
		public MemPtr GetAllocPtr(in Allocator allocator, uint index)
		{
			return allocator.RefArrayPtr(ptr.memPtr, ElementSize, index);
		}

		[INLINE(256)]
		public MemPtr GetAllocPtr(uint index)
		{
			var allocator = ptr.memPtr.GetAllocatorPtr();
			return allocator->RefArrayPtr(ptr.memPtr, ElementSize, index);
		}

		[INLINE(256)]
		public ref T GetValue<T>(in Allocator allocator, int index) where T: unmanaged
		{
			Debug.Assert(index >= 0 && index < Length);
			return ref *((T*)GetPtr(in allocator) + index);
		}

		[INLINE(256)]
		public ref T GetValue<T>(in Allocator allocator, uint index) where T: unmanaged
		{
			Debug.Assert(index < Length);
			return ref *((T*)GetPtr(in allocator) + index);
		}

		[INLINE(256)]
		public ref T GetValue<T>(int index) where T: unmanaged
		{
			return ref *((T*)GetPtr() + index);
		}

		[INLINE(256)]
		public ref T GetValue<T>(uint index) where T: unmanaged
		{
			return ref *((T*)GetPtr() + index);
		}

		[INLINE(256)]
		public T* GetValuePtr<T>() where T: unmanaged
		{
			return (T*)GetPtr(GetAllocator());
		}

		[INLINE(256)]
		public T* GetValuePtr<T>(in Allocator allocator) where T: unmanaged
		{
			return (T*)GetPtr(allocator);
		}

		[INLINE(256)]
		public T* GetValuePtr<T>(uint index) where T: unmanaged
		{
			return (T*)GetPtr(GetAllocator()) + index;
		}

		[INLINE(256)]
		public T* GetValuePtr<T>(in Allocator allocator, uint index) where T: unmanaged
		{
			return (T*)GetPtr(in allocator) + index;
		}

		[INLINE(256)]
		public T* GetValuePtr<T>(int index) where T: unmanaged
		{
			return (T*)GetPtr(GetAllocator()) + index;
		}

		[INLINE(256)]
		public T* GetValuePtr<T>(in Allocator allocator, int index) where T: unmanaged
		{
			return (T*)GetPtr(in allocator) + index;
		}

		[INLINE(256)]
		public void* GetValuePtr(in Allocator allocator)
		{
			return GetPtr(allocator);
		}

		[INLINE(256)]
		public void* GetValuePtr()
		{
			return GetPtr(GetAllocator());
		}

		[INLINE(256)]
		public void* GetValuePtr(uint index)
		{
			return (byte*)GetPtr(GetAllocator()) + (index * ElementSize);
		}

		[INLINE(256)]
		public void* GetValuePtr(int index)
		{
			return (byte*)GetPtr(GetAllocator()) + (index * ElementSize);
		}

		[INLINE(256)]
		public void* GetValuePtr(in Allocator allocator, uint index)
		{
			return (byte*)GetPtr(in allocator) + (index * ElementSize);
		}

		[INLINE(256)]
		public void* GetValuePtr(in Allocator allocator, int index)
		{
			return (byte*)GetPtr(in allocator) + (index * ElementSize);
		}

		[INLINE(256)]
		public bool Resize(ref Allocator allocator, uint newLength,
			ClearOptions options = ClearOptions.ClearMemory)
		{
			return Resize(ref allocator, newLength, growFactor, options);
		}

		[INLINE(256)]
		public bool Resize(ref Allocator allocator, uint newLength, ushort growFactor, ClearOptions options = ClearOptions.ClearMemory)
		{
			if (!IsCreated)
			{
				this = new MemArray(ref allocator, ElementSize, newLength, options, growFactor);
				return true;
			}

			if (newLength <= Length)
			{
				return false;
			}

			newLength *= growFactor;

			var prevLength = Length;
			var arrPtr = allocator.ReAllocArray(this.ptr.memPtr, ElementSize, newLength, out void* ptr);
			this.ptr = new Ptr(in allocator, ptr, arrPtr);
			if (options == ClearOptions.ClearMemory)
			{
				Clear(allocator, prevLength, newLength - prevLength);
			}

			Length = newLength;
			return true;
		}

		[INLINE(256)]
		public void Fill<T>(in Allocator allocator, in T value, uint from, uint to) where T: unmanaged
		{
			allocator.MemFill(ptr.memPtr, value, from, to - from);
		}

		[INLINE(256)]
		public void Clear(in Allocator allocator)
		{
			Clear(allocator, 0u, Length);
		}

		[INLINE(256)]
		public void Clear(in Allocator allocator, uint index, uint length)
		{
			var size = ElementSize;
			allocator.MemClear(ptr.memPtr, index * size, length * size);
		}

		[INLINE(256)]
		public void Clear()
		{
			Clear(0u, Length);
		}

		[INLINE(256)]
		public void Clear(uint index, uint length)
		{
			var size = ElementSize;
			var allocator = ptr.memPtr.GetAllocatorPtr();
			allocator->MemClear(ptr.memPtr, index * size, length * size);
		}

		[INLINE(256)]
		public bool Contains<T, U>(in Allocator allocator, in U obj) where T: unmanaged where U : unmanaged, System.IEquatable<T>
		{
			Debug.Assert(IsCreated);

			var ptr = (T*)GetPtr(in allocator);
			for (uint i = 0, cnt = Length; i < cnt; ++i)
			{
				if (obj.Equals(*(ptr + i)))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public uint GetReservedSizeInBytes()
		{
			return Length * ElementSize;
		}
	}
}
