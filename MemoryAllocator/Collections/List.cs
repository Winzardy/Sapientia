using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using Unity.Collections.LowLevel.Unsafe;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(ListProxy<>))]
	public unsafe struct List<T> : IIsCreated where T : unmanaged
	{
		public struct Enumerator
		{
			private readonly List<T> list;
			private uint index;

			internal Enumerator(in List<T> list)
			{
				this.list = list;
				index = 0u;
			}

			public bool MoveNext()
			{
				return index++ < list.Count;
			}

			public ref T GetCurrent(in Allocator allocator) => ref list[in allocator, index - 1u];
		}

		internal MemArray<T> arr;
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
		public List(ref Allocator allocator, uint capacity)
		{
			if (capacity <= 0u) capacity = 1u;
			this = default;
			EnsureCapacity(ref allocator, capacity);
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
			arr.BurstMode(in allocator, state);
		}

		[INLINE(256)]
		public void ReplaceWith(ref Allocator allocator, in List<T> other)
		{
			if (other.arr.cachedPtr.memPtr == arr.cachedPtr.memPtr)
			{
				return;
			}

			Dispose(ref allocator);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(ref Allocator allocator, in List<T> other)
		{
			if (other.arr.cachedPtr.memPtr == arr.cachedPtr.memPtr)
				return;
			if (!arr.cachedPtr.memPtr.IsValid() && !other.arr.cachedPtr.memPtr.IsValid())
				return;
			if (arr.cachedPtr.memPtr.IsValid() && !other.arr.cachedPtr.memPtr.IsValid())
			{
				Dispose(ref allocator);
				return;
			}

			if (!arr.cachedPtr.memPtr.IsValid())
				this = new List<T>(ref allocator, other.Capacity);

			NativeArrayUtils.Copy(ref allocator, in other.arr, ref arr);
			Count = other.Count;
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			E.IS_CREATED(this);
			return arr.cachedPtr.memPtr;
		}

		[INLINE(256)]
		public void* GetUnsafePtr(in Allocator allocator)
		{
			E.IS_CREATED(this);
			return arr.GetUnsafePtr(allocator);
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			E.IS_CREATED(this);
			arr.Dispose(ref allocator);
			this = default;
		}

		[INLINE(256)]
		public readonly Enumerator GetEnumerator()
		{
			if (IsCreated == false) return default;
			return new Enumerator(in this);
		}

		[INLINE(256)]
		public void Clear()
		{
			E.IS_CREATED(this);
			Count = 0;
		}

		public ref T this[in Allocator allocator, uint index]
		{
			[INLINE(256)]
			get
			{
				E.RANGE(index, 0, Count);
				return ref arr[in allocator, index];
			}
		}

		[INLINE(256)]
		private bool EnsureCapacity(ref Allocator allocator, uint capacity)
		{
			capacity = Helpers.NextPot(capacity);
			if (arr.IsCreated == false) arr.growFactor = 1;
			return arr.Resize(ref allocator, capacity, ClearOptions.UninitializedMemory);
		}

		[INLINE(256)]
		public uint Add(ref Allocator allocator, T obj)
		{
			E.IS_CREATED(this);
			++Count;
			EnsureCapacity(ref allocator, Count);

			arr[in allocator, Count - 1u] = obj;
			return Count - 1u;
		}

		[INLINE(256)]
		public readonly bool Contains<U>(in Allocator allocator, U obj) where U : unmanaged, System.IEquatable<T>
		{
			E.IS_CREATED(this);
			for (uint i = 0, cnt = Count; i < cnt; ++i)
			{
				if (obj.Equals(arr[in allocator, i]))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool Remove<U>(ref Allocator allocator, U obj) where U : unmanaged, System.IEquatable<T>
		{
			E.IS_CREATED(this);
			for (uint i = 0, cnt = Count; i < cnt; ++i)
			{
				if (obj.Equals(arr[in allocator, i]))
				{
					RemoveAt(ref allocator, i);
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool RemoveFast<U>(in Allocator allocator, U obj) where U : unmanaged, System.IEquatable<T>
		{
			E.IS_CREATED(this);
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
		public unsafe bool RemoveAt(ref Allocator allocator, uint index)
		{
			E.IS_CREATED(this);
			if (index >= Count) return false;

			if (index == Count - 1)
			{
				--Count;
				arr[in allocator, Count] = default;
				return true;
			}

			var ptr = arr.cachedPtr.memPtr;
			var size = sizeof(T);
			allocator.MemMove(ptr, size * index, ptr, size * (index + 1), (Count - index - 1) * size);

			--Count;
			arr[in allocator, Count] = default;

			return true;
		}

		[INLINE(256)]
		public bool RemoveAtFast(in Allocator allocator, uint index)
		{
			E.IS_CREATED(this);
			if (index >= Count) return false;

			--Count;
			var last = arr[in allocator, Count];
			arr[in allocator, index] = last;

			return true;
		}

		[INLINE(256)]
		public bool Resize(ref Allocator allocator, uint newLength)
		{
			if (IsCreated == false)
			{
				this = new List<T>(ref allocator, newLength);
				return true;
			}

			if (newLength <= Capacity)
			{
				return false;
			}

			return EnsureCapacity(ref allocator, newLength);
		}

		[INLINE(256)]
		public void AddRange(ref Allocator allocator, in List<T> collection)
		{
			AddRange(ref allocator, in collection, 0u, collection.Count);
		}

		[INLINE(256)]
		public void AddRange(ref Allocator allocator, in List<T> collection, uint fromIdx, uint toIdx)
		{
			E.IS_CREATED(this);
			E.IS_CREATED(collection);

			var index = Count;

			var srcOffset = fromIdx;
			var count = toIdx - fromIdx;
			if (count > 0u)
			{
				EnsureCapacity(ref allocator, Count + count);
				var size = sizeof(T);
				if (index < Count)
				{
					allocator.MemMove(arr.cachedPtr.memPtr, (index + count) * size, arr.cachedPtr.memPtr, index * size,
						(Count - index) * size);
				}

				if (arr.cachedPtr.memPtr == collection.arr.cachedPtr.memPtr)
				{
					allocator.MemMove(arr.cachedPtr.memPtr, index * size, arr.cachedPtr.memPtr, 0, index * size);
					allocator.MemMove(arr.cachedPtr.memPtr, (index * 2) * size, arr.cachedPtr.memPtr, (index + count) * size,
						(Count - index) * size);
				}
				else
				{
					collection.CopyTo(ref allocator, arr, srcOffset, index, count);
				}

				Count += count;
			}
		}

		[INLINE(256)]
		public void AddRange(ref Allocator allocator, MemArray<T> collection)
		{
			E.IS_CREATED(this);
			E.IS_CREATED(collection);

			var index = Count;
			var count = collection.Length;
			if (count > 0u)
			{
				EnsureCapacity(ref allocator, Count + count);
				var size = sizeof(T);
				if (index < Count)
				{
					allocator.MemMove(arr.cachedPtr.memPtr, (index + count) * size, arr.cachedPtr.memPtr, index * size,
						(Count - index) * size);
				}

				if (arr.cachedPtr.memPtr == collection.cachedPtr.memPtr)
				{
					allocator.MemMove(arr.cachedPtr.memPtr, index * size, arr.cachedPtr.memPtr, 0, index * size);
					allocator.MemMove(arr.cachedPtr.memPtr, (index * 2) * size, arr.cachedPtr.memPtr, (index + count) * size,
						(Count - index) * size);
				}
				else
				{
					CopyFrom(ref allocator, collection, index);
				}

				Count += count;
			}
		}

		[INLINE(256)]
		public void AddRange(ref Allocator allocator, Unity.Collections.NativeArray<T> collection)
		{
			E.IS_CREATED(this);

			var index = Count;
			var count = (uint)collection.Length;
			if (count > 0u)
			{
				EnsureCapacity(ref allocator, Count + count);
				var size = sizeof(T);
				MemoryExt.MemCopy(collection.GetUnsafeReadOnlyPtr(),
					(byte*)arr.GetUnsafePtr(in allocator) + index * size, count * size);
				Count += count;
			}
		}

		[INLINE(256)]
		public readonly void CopyTo(ref Allocator allocator, MemArray<T> arr, uint srcOffset, uint index, uint count)
		{
			E.IS_CREATED(this);
			E.IS_CREATED(arr);

			var size = sizeof(T);
			allocator.MemCopy(arr.cachedPtr.memPtr, index * size, this.arr.cachedPtr.memPtr, srcOffset * size, count * size);
		}

		[INLINE(256)]
		public readonly void CopyTo(ref Allocator allocator, in MemPtr arrPtr, uint srcOffset, uint index, uint count)
		{
			E.IS_CREATED(this);

			var size = sizeof(T);
			allocator.MemCopy(arrPtr, index * size, arr.cachedPtr.memPtr, srcOffset * size, count * size);
		}

		[INLINE(256)]
		public readonly void CopyFrom(ref Allocator allocator, MemArray<T> arr, uint index)
		{
			E.IS_CREATED(this);
			E.IS_CREATED(arr);

			var size = sizeof(T);
			allocator.MemCopy(this.arr.cachedPtr.memPtr, index * size, arr.cachedPtr.memPtr, 0, arr.Length * size);
		}

		public uint GetReservedSizeInBytes()
		{
			return arr.GetReservedSizeInBytes();
		}
	}
}
