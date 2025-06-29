using System;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MemArray
	{
		public static readonly MemArray Empty = new () { ptr = CachedPtr.Invalid, Length = 0, };

		public CachedPtr ptr;
		public int Length { get; private set; }
		public int ElementSize { get; private set; }

		public int Count
		{
			[INLINE(256)] get => Length;
		}

		public readonly bool IsCreated
		{
			[INLINE(256)] get => ptr.IsValid();
		}

		[INLINE(256)]
		public MemArray(int elementSize, int length, ClearOptions clearOptions = ClearOptions.ClearMemory) : this(WorldManager.CurrentWorldState, elementSize, length, clearOptions)
		{

		}

		[INLINE(256)]
		public MemArray(WorldState worldState, int elementSize, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			this = default;
			ptr = default;
			var memPtr = worldState.MemAlloc(elementSize * length, out var tPtr);
			ptr = new CachedPtr(worldState, tPtr, memPtr);
			ElementSize = elementSize;
			Length = length;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				Clear(worldState);
			}
		}

		[INLINE(256)]
		public MemArray(in MemArray arr) : this(WorldManager.CurrentWorldState, arr)
		{
		}

		[INLINE(256)]
		public MemArray(WorldState worldState, in MemArray arr)
		{
			if (arr.Length == 0u)
			{
				this = Empty;
				return;
			}

			ptr = default;
			Length = arr.Length;
			ElementSize = arr.ElementSize;

			var memPtr = worldState.MemAlloc(arr.ElementSize * arr.Length, out var tPtr);
			ptr = new CachedPtr(worldState, tPtr, memPtr);
			MemArrayExt.CopyNoChecks(worldState, in arr, 0, ref this, 0, arr.Length);
		}

		[INLINE(256)]
		public void ReplaceWith(WorldState worldState, in MemArray other)
		{
			if (other.ptr.memPtr == ptr.memPtr)
			{
				return;
			}

			Dispose(worldState);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(WorldState worldState, in MemArray other)
		{
			if (other.ptr.memPtr == ptr.memPtr) return;
			if (!ptr.memPtr.IsValid() && !other.ptr.memPtr.IsValid())
				return;
			if (ptr.memPtr.IsValid() && !other.ptr.memPtr.IsValid())
			{
				Dispose(worldState);
				return;
			}

			if (!ptr.memPtr.IsValid())
				this = new MemArray(worldState, other.ElementSize, other.Length, ClearOptions.ClearMemory);

			MemArrayExt.Copy(worldState, in other, ref this);
		}

		[INLINE(256)]
		public void Dispose(WorldState worldState)
		{
			if (ptr.memPtr.IsValid())
			{
				worldState.MemFree(ptr.memPtr);
			}

			this = default;
		}

		[INLINE(256)]
		public SafePtr GetPtr(WorldState worldState)
		{
			return ptr.GetPtr(worldState);
		}

		[INLINE(256)]
		public MemPtr GetWPtr(int index)
		{
			return ptr.memPtr.GetArrayElement(ElementSize, index);
		}

		[INLINE(256)]
		public ref T GetValue<T>(WorldState worldState, int index) where T: unmanaged
		{
			return ref (GetPtr(worldState).Cast<T>() + index).Value();
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr<T>(WorldState worldState) where T: unmanaged
		{
			return GetPtr(worldState).Cast<T>();
		}

		public Span<T> GetSpan<T>(WorldState worldState) where T: unmanaged
		{
			return new Span<T>(GetValuePtr<T>(worldState).ptr, Length);
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr<T>(WorldState worldState, int index) where T: unmanaged
		{
			return GetPtr(worldState).Cast<T>() + index;
		}

		[INLINE(256)]
		public SafePtr GetValuePtr(WorldState worldState)
		{
			return GetPtr(worldState);
		}

		[INLINE(256)]
		public SafePtr GetValuePtr(WorldState worldState, int index)
		{
			return GetPtr(worldState) + (index * ElementSize);
		}

		[INLINE(256)]
		public bool Resize(WorldState worldState, int newLength, int elementSize, ClearOptions options = ClearOptions.ClearMemory)
		{
			if (!IsCreated)
			{
				this = new MemArray(worldState, elementSize, newLength, options);
				return true;
			}

			if (newLength <= Length)
			{
				return false;
			}

			E.ASSERT(elementSize == ElementSize);

			var prevLength = Length;
			var arrPtr = worldState.MemReAlloc(ptr.memPtr, elementSize * newLength, out var rawPtr);
			ptr = new CachedPtr(worldState, rawPtr, arrPtr);

			if (options == ClearOptions.ClearMemory)
			{
				Clear(worldState, prevLength, newLength - prevLength);
			}

			Length = newLength;
			return true;
		}

		[INLINE(256)]
		public bool Resize<T>(WorldState worldState, int newLength, ClearOptions options = ClearOptions.ClearMemory)
			where T : unmanaged
		{
			var elementSize = TSize<T>.size;
			return Resize(worldState, newLength, elementSize, options);
		}

		[INLINE(256)]
		public void Fill<T>(WorldState worldState, in T value, int fromIndex, int count) where T: unmanaged
		{
			worldState.MemFill<T>(ptr.memPtr, value, fromIndex, count);
		}

		[INLINE(256)]
		public void Clear(WorldState worldState)
		{
			Clear(worldState, 0, Length);
		}

		[INLINE(256)]
		public void Clear(WorldState worldState, int index, int length)
		{
			var size = ElementSize;
			worldState.MemClear(ptr.memPtr, index * size, length * size);
		}

		[INLINE(256)]
		public bool Contains<T, TU>(WorldState worldState, in TU obj) where T: unmanaged where TU : unmanaged, IEquatable<T>
		{
			E.ASSERT(IsCreated);

			var safePtr = GetPtr(worldState).Cast<T>();
			for (int i = 0, cnt = Length; i < cnt; ++i)
			{
				if (obj.Equals((safePtr + i).Value()))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public int GetReservedSizeInBytes()
		{
			return Length * ElementSize;
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator<T>(WorldState worldState) where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(worldState), Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator<T> GetPtrEnumerator<T>(WorldState worldState) where T: unmanaged
		{
			return new ListPtrEnumerator<T>(GetValuePtr(worldState), ElementSize, Count);
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>(WorldState worldState) where T: unmanaged
		{
			return new (new (GetValuePtr<T>(worldState), Count));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable<T>(WorldState worldState) where T: unmanaged
		{
			return new (new (GetValuePtr(worldState), ElementSize, Count));
		}
	}
}
