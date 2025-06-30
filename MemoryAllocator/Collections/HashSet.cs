using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxy(typeof(HashSet<>.HashSetProxy))]
	public struct HashSet<T> : IHashSetEnumerable<T>
		where T : unmanaged, IEquatable<T>
	{
		public struct Slot
		{
			internal int hashCode; // Lower 31 bits of hash code, -1 if unused
			internal int next; // Index of next entry, -1 if last
			internal T value;
		}

		private const int _hashCodeMask = 0x7FFFFFFF;

		internal MemArray<int> buckets;
		internal MemArray<Slot> slots;
		internal int count;
		internal int lastIndex;
		internal int freeList;
		internal int hash;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => buckets.IsCreated;
		}

		public int Count
		{
			[INLINE(256)] get => count;
		}

		public int LastIndex
		{
			[INLINE(256)] get => lastIndex;
		}

		[INLINE(256)]
		public HashSet(WorldState worldState, int capacity = 8)
		{
			this = default;
			Initialize(worldState, capacity);
		}

		[INLINE(256)]
		public HashSet(WorldState worldState, in HashSet<T> other)
		{
			E.ASSERT(other.IsCreated);

			this = other;
			buckets = new MemArray<int>(worldState, other.buckets);
			slots = new MemArray<Slot>(worldState, other.slots);
		}

		[INLINE(256)]
		public HashSet(WorldState worldState, in ICollection<T> other) : this(worldState, other.Count)
		{
			foreach (var value in other)
			{
				Add(worldState, value);
			}
		}

		[INLINE(256)]
		public HashSet(WorldState worldState, in IEnumerable<T> other, int capacity) : this(worldState, capacity)
		{
			foreach (var value in other)
			{
				Add(worldState, value);
			}
		}

		/// <summary>
		/// Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
		/// greater than or equal to capacity.
		/// </summary>
		/// <param name="worldator"></param>
		/// <param name="capacity"></param>
		[INLINE(256)]
		private void Initialize(WorldState worldState, int capacity)
		{
			var size = capacity.GetPrime();
			buckets = new MemArray<int>(worldState, size);
			slots = new MemArray<Slot>(worldState, size);
			freeList = -1;
		}

		[INLINE(256)]
		public SafePtr<Slot> GetSlotPtr(WorldState worldState)
		{
			E.ASSERT(IsCreated);
			return slots.GetValuePtr(worldState);
		}

		[INLINE(256)]
		public void Dispose(WorldState worldState)
		{
			buckets.Dispose(worldState);
			slots.Dispose(worldState);
			this = default;
		}

		[INLINE(256)]
		public MemPtr GetMemPtr()
		{
			E.ASSERT(IsCreated);
			return buckets.innerArray.ptr.memPtr;
		}

		[INLINE(256)]
		public void ReplaceWith(WorldState worldState, ref HashSet<T> other)
		{
			if (GetMemPtr() == other.GetMemPtr())
			{
				return;
			}

			Dispose(worldState);
			this = other;
		}

		/// <summary>
		/// Remove all items from this set. This clears the elements but not the underlying
		/// buckets and slots array. Follow this call by TrimExcess to release these.
		/// </summary>
		/// <param name="worldator"></param>
		[INLINE(256)]
		public void Clear(WorldState worldState)
		{
			if (lastIndex > 0)
			{
				// clear the elements so that the gc can reclaim the references.
				// clear only up to m_lastIndex for m_slots
				slots.Clear(worldState, 0, lastIndex);
				buckets.Clear(worldState, 0, buckets.Length);
				lastIndex = 0;
				count = 0;
				freeList = -1;
				hash = 0;
			}
		}

		/// <summary>
		/// Checks if this hashset contains the item
		/// </summary>
		/// <param name="worldator"></param>
		/// <param name="item">item to check for containment</param>
		/// <returns>true if item contained; false if not</returns>
		[INLINE(256)]
		public bool Contains(WorldState worldState, in T item)
		{
			return Contains(item, slots.GetValuePtr(worldState), buckets.GetValuePtr(worldState));
		}

		[INLINE(256)]
		private readonly bool Contains(in T item, SafePtr<Slot> slotsPtr, SafePtr<int> bucketsPtr)
		{
			E.ASSERT(IsCreated);

			var hashCode = item.GetHashCode() & _hashCodeMask;
			// see note at "HashSet" level describing why "- 1" appears in for loop
			for (var i = bucketsPtr[hashCode % buckets.Length] - 1; i >= 0; i = slotsPtr[i].next)
			{
				if (slotsPtr[i].hashCode == hashCode && slotsPtr[i].value.Equals(item))
				{
					return true;
				}
			}
			// either m_buckets is null or wasn't found
			return false;
		}

		[INLINE(256)]
		public void Add(WorldState worldState, ref HashSet<T> other)
		{
			var slotsPtr = slots.GetValuePtr(worldState);
			for (var i = 0; i < other.lastIndex; i++)
			{
				ref var slot = ref (slotsPtr + i).Value();
				if (slot.hashCode >= 0)
				{
					Add(worldState, slot.value);
				}
			}
		}

		[INLINE(256)]
		public void RemoveExcept(WorldState worldState, ref HashSet<T> other)
		{
			var slotsPtr = slots.GetValuePtr(worldState);
			for (var i = 0; i < lastIndex; i++)
			{
				ref var slot = ref slotsPtr[i];
				if (slot.hashCode >= 0)
				{
					var item = slot.value;
					if (!other.Contains(worldState, item))
					{
						Remove(worldState, item);
						if (count == 0)
							return;
					}
				}
			}
		}

		[INLINE(256)]
		public void Remove(WorldState worldState, ref HashSet<T> other)
		{
			E.ASSERT(IsCreated);

			var slotsPtr = slots.GetValuePtr(worldState);
			for (var i = 0; i < lastIndex; i++)
			{
				ref var slot = ref (slotsPtr + i).Value();
				if (slot.hashCode >= 0)
				{
					var item = slot.value;
					if (other.Contains(worldState, item))
					{
						Remove(worldState, item);
						if (count == 0)
							return;
					}
				}
			}
		}

		/// <summary>
		/// Remove item from this hashset
		/// </summary>
		/// <param name="worldator"></param>
		/// <param name="item">item to remove</param>
		/// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
		[INLINE(256)]
		public bool Remove(WorldState worldState, in T item)
		{
			if (!buckets.IsCreated)
				return false;

			var slotsPtr = slots.GetValuePtr(worldState);
			var bucketsPtr = buckets.GetValuePtr(worldState);

			var hashCode = item.GetHashCode() & _hashCodeMask;
			var bucket = hashCode % buckets.Length;
			var last = -1;
			for (var i = bucketsPtr[bucket] - 1; i >= 0; last = i, i = slotsPtr[i].next)
			{
				if (slotsPtr[i].hashCode == hashCode && slotsPtr[i].value.Equals(item))
				{
					if (last < 0)
					{
						// first iteration; update buckets
						bucketsPtr[bucket] = slotsPtr[i].next + 1;
					}
					else
					{
						// subsequent iterations; update 'next' pointers
						slotsPtr[last].next = slotsPtr[i].next;
					}
					slotsPtr[i].hashCode = -1;
					slotsPtr[i].value = default(T);
					slotsPtr[i].next = freeList;

					count--;
					if (count == 0)
					{
						lastIndex = 0;
						freeList = -1;
					}
					else
					{
						freeList = i;
					}
					return true;
				}
			}
			// either m_buckets is null or wasn't found
			return false;
		}

		/// <summary>
		/// Expand to new capacity. New capacity is next prime greater than or equal to suggested
		/// size. This is called when the underlying array is filled. This performs no
		/// defragmentation, allowing faster execution; note that this is reasonable since
		/// AddIfNotPresent attempts to insert new elements in re-opened spots.
		/// </summary>
		/// <param name="worldator"></param>
		[INLINE(256)]
		private void IncreaseCapacity(WorldState worldState)
		{
			var newSize = count.ExpandPrime();

			// Able to increase capacity; copy elements to larger array and rehash
			SetCapacity(worldState, newSize);
		}

		/// <summary>
		/// Set the underlying buckets array to size newSize and rehash.  Note that newSize
		/// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
		/// instead of this method.
		/// </summary>
		[INLINE(256)]
		private void SetCapacity(WorldState worldState, int newSize)
		{
			var newSlots = new MemArray<Slot>(worldState, newSize);
			if (slots.IsCreated)
			{
				MemArrayExt.CopyNoChecks<HashSet<T>.Slot>(worldState, slots, 0, ref newSlots, 0, lastIndex);
			}

			var newBuckets = new MemArray<int>(worldState, newSize);
			for (var i = 0; i < lastIndex; i++)
			{
				var bucket = newSlots[worldState, i].hashCode % newSize;
				newSlots[worldState, i].next = newBuckets[worldState, bucket] - 1;
				newBuckets[worldState, bucket] = i + 1;
			}

			if (slots.IsCreated)
				slots.Dispose(worldState);
			if (buckets.IsCreated)
				buckets.Dispose(worldState);
			slots = newSlots;
			buckets = newBuckets;
		}

		/// <summary>
		/// Add item to this HashSet. Returns bool indicating whether item was added (won't be
		/// added if already present)
		/// </summary>
		/// <param name="worldator"></param>
		/// <param name="value"></param>
		/// <returns>true if added, false if already present</returns>
		[INLINE(256)]
		public bool Add(WorldState worldState, in T value)
		{
			if (!buckets.IsCreated)
			{
				Initialize(worldState, 0);
			}

			var bucketsPtr = buckets.GetValuePtr(worldState);
			var slotsPtr = slots.GetValuePtr(worldState);

			var hashCode = value.GetHashCode() & _hashCodeMask;
			var bucket = hashCode % buckets.Length;

			for (var i = bucketsPtr[bucket] - 1; i >= 0; i = slotsPtr[i].next)
			{
				if (slotsPtr[i].hashCode == hashCode && slotsPtr[i].value.Equals(value))
				{
					return false;
				}
			}

			int index;
			if (freeList >= 0)
			{
				index = freeList;
				freeList = slotsPtr[index].next;
			}
			else
			{
				if (lastIndex == slots.Length)
				{
					IncreaseCapacity(worldState);
					// this will change during resize
					bucket = hashCode % buckets.Length;

					bucketsPtr = buckets.GetValuePtr(worldState);
					slotsPtr = slots.GetValuePtr(worldState);
				}
				index = lastIndex;
				lastIndex++;
			}

			slotsPtr[index].hashCode = hashCode;
			slotsPtr[index].value = value;
			slotsPtr[index].next = bucketsPtr[bucket] - 1;
			bucketsPtr[bucket] = index + 1;

			count++;

			return true;
		}

		[INLINE(256)]
		public readonly int GetHash()
		{
			return hash;
		}

		[INLINE(256)]
		public void CopyFrom(WorldState worldState, in HashSet<T> other)
		{
			MemArrayExt.CopyExact(worldState, in other.buckets, ref buckets);

			slots.CopyFrom(worldState, other.slots);
			var thisBuckets = buckets;
			var thisSlots = slots;
			this = other;

			buckets = thisBuckets;
			slots = thisSlots;
		}

		[INLINE(256)]
		public HashSetEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new HashSetEnumerator<T>(GetSlotPtr(worldState), LastIndex);
		}

		[INLINE(256)]
		public HashSetPtrEnumerator<T> GetPtrEnumerator(WorldState worldState)
		{
			return new HashSetPtrEnumerator<T>(GetSlotPtr(worldState), LastIndex);
		}

		[INLINE(256)]
		public Enumerable<T, HashSetEnumerator<T>> GetEnumerable(WorldState worldState)
		{
			return new (new (GetSlotPtr(worldState), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, HashSetPtrEnumerator<T>> GetPtrEnumerable(WorldState worldState)
		{
			return new (new (GetSlotPtr(worldState), LastIndex));
		}

		private class HashSetProxy
		{
			private HashSet<T> _hashSet;

			public HashSetProxy(HashSet<T> hashSet)
			{
				_hashSet = hashSet;
			}

			public MemArray<int> Buckets => _hashSet.buckets;
			public MemArray<HashSet<T>.Slot> Slots => _hashSet.slots;
			public int Count => _hashSet.count;
			public int FreeList => _hashSet.freeList;
			public int LastIndex => _hashSet.lastIndex;

			public T[] Items
			{
				get
				{
#if DEBUG
					var arr = new T[_hashSet.Count];
					var i = 0;
					var worldState = _hashSet.buckets.GetWorldState_DEBUG();
					var e = _hashSet.GetEnumerator(worldState);
					while (e.MoveNext())
					{
						arr[i++] = e.Current;
					}

					e.Dispose();

					return arr;
#else
					return Array.Empty<T>();
#endif
				}
			}
		}
	}
}
