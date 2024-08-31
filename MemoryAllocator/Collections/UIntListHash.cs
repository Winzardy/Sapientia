using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public unsafe struct UIntListHash : IIsCreated
	{
		private MemArray<uint> arr;
		public uint hash;
		public uint Count;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => arr.IsCreated;
		}

		public uint Capacity
		{
			[INLINE(256)]
			get
			{
				E.IS_CREATED(this);
				return arr.Length;
			}
		}

		[INLINE(256)]
		public UIntListHash(ref Allocator allocator, uint capacity, ushort growFactor = 1)
		{
			if (capacity <= 0u) capacity = 1u;
			this = default;
			arr.innerArray.growFactor = growFactor;
			EnsureCapacity(ref allocator, capacity);
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
			arr.BurstMode(in allocator, state);
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			return arr.innerArray.ptr.memPtr;
		}

		[INLINE(256)]
		public void* GetUnsafePtr(in Allocator allocator)
		{
			return arr.GetPtr(in allocator);
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			arr.Dispose(ref allocator);
			this = default;
		}

		[INLINE(256)]
		public void Clear()
		{
			Count = 0u;
			hash = 0u;
		}

		public ref uint this[in Allocator allocator, uint index]
		{
			[INLINE(256)]
			get => ref arr[in allocator, index];
		}

		[INLINE(256)]
		public bool EnsureCapacity(ref Allocator allocator, uint capacity)
		{
			capacity = Helpers.NextPot(capacity);
			if (!arr.IsCreated)
				arr.innerArray.growFactor = 1;
			return arr.Resize(ref allocator, capacity, ClearOptions.UninitializedMemory);
		}

		[INLINE(256)]
		public uint Add(ref Allocator allocator, uint obj)
		{
			++Count;
			EnsureCapacity(ref allocator, Count);

			hash ^= obj;
			arr[in allocator, Count - 1u] = obj;
			return Count - 1u;
		}

		[INLINE(256)]
		private void AddNoCheck(in Allocator allocator, uint obj)
		{
			hash ^= obj;
			arr[in allocator, Count] = obj;
			++Count;
		}

		[INLINE(256)]
		public bool RemoveFast(in Allocator allocator, uint obj)
		{
			for (uint i = 0, cnt = Count; i < cnt; ++i)
			{
				if (obj.Equals(arr[in allocator, i]))
				{
					RemoveAtFast(in allocator, i);
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool RemoveAtFast(in Allocator allocator, uint index)
		{
			if (index >= Count) return false;

			hash ^= arr[in allocator, index];
			--Count;
			var last = arr[in allocator, Count];
			arr[in allocator, index] = last;

			return true;
		}

		[INLINE(256)]
		public readonly void CopyTo(ref Allocator allocator, in MemPtr arrPtr, uint srcOffset, uint index, uint count)
		{
			const int size = sizeof(uint);
			allocator.MemCopy(arrPtr, index * size, arr.innerArray.ptr.memPtr, srcOffset * size, count * size);
		}

		[INLINE(256)]
		public readonly uint GetHash()
		{
			return hash;
		}

		/*[INLINE(256)]
		public void AddRange(ref Allocator allocator, in UIntHashSet collection)
		{
			EnsureCapacity(ref allocator, Count + collection.Count);
			var e = collection.GetEnumerator(in allocator);
			while (e.MoveNext())
			{
				var val = e.Current;
				AddNoCheck(in allocator, val);
			}
		}*/

		[INLINE(256)]
		public void AddRange(ref Allocator allocator, in UIntListHash collection, uint fromIdx, uint toIdx)
		{
			var index = Count;

			var srcOffset = fromIdx;
			var count = toIdx - fromIdx;
			if (count > 0u)
			{
				EnsureCapacity(ref allocator, Count + count);
				var size = TSize<uint>.size;
				if (index < Count)
				{
					allocator.MemMove(arr.innerArray.ptr.memPtr, (index + count) * size, arr.innerArray.ptr.memPtr, index * size,
						(Count - index) * size);
				}

				if (arr.innerArray.ptr.memPtr == collection.arr.innerArray.ptr.memPtr)
				{
					allocator.MemMove(arr.innerArray.ptr.memPtr, index * size, arr.innerArray.ptr.memPtr, 0, index * size);
					allocator.MemMove(arr.innerArray.ptr.memPtr, (index * 2) * size, arr.innerArray.ptr.memPtr, (index + count) * size,
						(Count - index) * size);
				}
				else
				{
					collection.CopyTo(ref allocator, arr.innerArray.ptr.memPtr, srcOffset, index, count);
				}

				Count += count;
			}
		}

		public uint[] ToManagedArray(in Allocator allocator)
		{
			var dst = new uint[Count];

			CopySafe(in allocator, this, 0, dst, 0, Count);
			return dst;
		}

		private static void CopySafe(
			in Allocator allocator,
			UIntListHash src,
			int srcIndex,
			uint[] dst,
			int dstIndex,
			uint length)
		{
			var gcHandle =
				System.Runtime.InteropServices.GCHandle.Alloc(dst, System.Runtime.InteropServices.GCHandleType.Pinned);
			MemoryExt.MemCopy((void*)((System.IntPtr)src.GetUnsafePtr(in allocator) + srcIndex * TSize<uint>.size),
				(void*)((System.IntPtr)(void*)gcHandle.AddrOfPinnedObject() + dstIndex * TSize<uint>.size),
				length * TSize<uint>.size);
			gcHandle.Free();
		}

		public bool Contains(in Allocator allocator, uint value)
		{
			for (uint i = 0; i < Count; ++i)
			{
				if (this[in allocator, i] == value) return true;
			}

			return false;
		}

		public uint GetReservedSizeInBytes()
		{
			return arr.GetReservedSizeInBytes();
		}
	}
}
