using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxy(typeof(HashSetProxy<>))]
	public unsafe struct HashSet<T> : IHashSetEnumerable<T>
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

#if UNITY_EDITOR
		public World GetWorld()
		{
			return buckets.GetWorld();
		}
#endif

		[INLINE(256)]
		public HashSet(World world, int capacity = 8)
		{
			this = default;
			Initialize(world, capacity);
		}

		[INLINE(256)]
		public HashSet(World world, in HashSet<T> other)
		{
			E.ASSERT(other.IsCreated);

			this = other;
			buckets = new MemArray<int>(world, other.buckets);
			slots = new MemArray<Slot>(world, other.slots);
		}

		[INLINE(256)]
		public HashSet(World world, in ICollection<T> other) : this(world, other.Count)
		{
			foreach (var value in other)
			{
				Add(world, value);
			}
		}

		[INLINE(256)]
		public HashSet(World world, in IEnumerable<T> other, int capacity) : this(world, capacity)
		{
			foreach (var value in other)
			{
				Add(world, value);
			}
		}

		/// <summary>
		/// Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
		/// greater than or equal to capacity.
		/// </summary>
		/// <param name="worldator"></param>
		/// <param name="capacity"></param>
		[INLINE(256)]
		private void Initialize(World world, int capacity)
		{
			var size = capacity.GetPrime();
			buckets = new MemArray<int>(world, size);
			slots = new MemArray<Slot>(world, size);
			freeList = -1;
		}

		[INLINE(256)]
		public SafePtr<Slot> GetSlotPtr(World world)
		{
			E.ASSERT(IsCreated);
			return slots.GetValuePtr(world);
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			buckets.Dispose(world);
			slots.Dispose(world);
			this = default;
		}

		[INLINE(256)]
		public MemPtr GetMemPtr()
		{
			E.ASSERT(IsCreated);
			return buckets.innerArray.ptr.memPtr;
		}

		[INLINE(256)]
		public void ReplaceWith(World world, ref HashSet<T> other)
		{
			if (GetMemPtr() == other.GetMemPtr())
			{
				return;
			}

			Dispose(world);
			this = other;
		}

		/// <summary>
		/// Remove all items from this set. This clears the elements but not the underlying
		/// buckets and slots array. Follow this call by TrimExcess to release these.
		/// </summary>
		/// <param name="worldator"></param>
		[INLINE(256)]
		public void Clear(World world)
		{
			if (lastIndex > 0)
			{
				// clear the elements so that the gc can reclaim the references.
				// clear only up to m_lastIndex for m_slots
				slots.Clear(world, 0, lastIndex);
				buckets.Clear(world, 0, buckets.Length);
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
		public bool Contains(World world, in T item)
		{
			return Contains(item, slots.GetValuePtr(world), buckets.GetValuePtr(world));
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
		public void Add(World world, ref HashSet<T> other)
		{
			var slotsPtr = slots.GetValuePtr(world);
			for (var i = 0; i < other.lastIndex; i++)
			{
				ref var slot = ref (slotsPtr + i).Value();
				if (slot.hashCode >= 0)
				{
					Add(world, slot.value);
				}
			}
		}

		[INLINE(256)]
		public void RemoveExcept(World world, ref HashSet<T> other)
		{
			var slotsPtr = slots.GetValuePtr(world);
			for (var i = 0; i < lastIndex; i++)
			{
				ref var slot = ref slotsPtr[i];
				if (slot.hashCode >= 0)
				{
					var item = slot.value;
					if (!other.Contains(world, item))
					{
						Remove(world, item);
						if (count == 0)
							return;
					}
				}
			}
		}

		[INLINE(256)]
		public void Remove(World world, ref HashSet<T> other)
		{
			E.ASSERT(IsCreated);

			var slotsPtr = slots.GetValuePtr(world);
			for (var i = 0; i < lastIndex; i++)
			{
				ref var slot = ref (slotsPtr + i).Value();
				if (slot.hashCode >= 0)
				{
					var item = slot.value;
					if (other.Contains(world, item))
					{
						Remove(world, item);
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
		public bool Remove(World world, in T item)
		{
			if (!buckets.IsCreated)
				return false;

			var slotsPtr = slots.GetValuePtr(world);
			var bucketsPtr = buckets.GetValuePtr(world);

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
		private void IncreaseCapacity(World world)
		{
			var newSize = count.ExpandPrime();

			// Able to increase capacity; copy elements to larger array and rehash
			SetCapacity(world, newSize);
		}

		/// <summary>
		/// Set the underlying buckets array to size newSize and rehash.  Note that newSize
		/// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
		/// instead of this method.
		/// </summary>
		[INLINE(256)]
		private void SetCapacity(World world, int newSize)
		{
			var newSlots = new MemArray<Slot>(world, newSize);
			if (slots.IsCreated)
			{
				MemArrayExt.CopyNoChecks<HashSet<T>.Slot>(world, slots, 0, ref newSlots, 0, lastIndex);
			}

			var newBuckets = new MemArray<int>(world, newSize);
			for (var i = 0; i < lastIndex; i++)
			{
				var bucket = newSlots[world, i].hashCode % newSize;
				newSlots[world, i].next = newBuckets[world, bucket] - 1;
				newBuckets[world, bucket] = i + 1;
			}

			if (slots.IsCreated)
				slots.Dispose(world);
			if (buckets.IsCreated)
				buckets.Dispose(world);
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
		public bool Add(World world, in T value)
		{
			if (!buckets.IsCreated)
			{
				Initialize(world, 0);
			}

			var bucketsPtr = buckets.GetValuePtr(world);
			var slotsPtr = slots.GetValuePtr(world);

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
					IncreaseCapacity(world);
					// this will change during resize
					bucket = hashCode % buckets.Length;

					bucketsPtr = buckets.GetValuePtr(world);
					slotsPtr = slots.GetValuePtr(world);
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
		public void CopyFrom(World world, in HashSet<T> other)
		{
			MemArrayExt.CopyExact(world, in other.buckets, ref buckets);

			slots.CopyFrom(world, other.slots);
			var thisBuckets = buckets;
			var thisSlots = slots;
			this = other;

			buckets = thisBuckets;
			slots = thisSlots;
		}

		[INLINE(256)]
		public HashSetEnumerator<T> GetEnumerator(World world)
		{
			return new HashSetEnumerator<T>(GetSlotPtr(world), LastIndex);
		}

		[INLINE(256)]
		public HashSetPtrEnumerator<T> GetPtrEnumerator(World world)
		{
			return new HashSetPtrEnumerator<T>(GetSlotPtr(world), LastIndex);
		}

		[INLINE(256)]
		public Enumerable<T, HashSetEnumerator<T>> GetEnumerable(World world)
		{
			return new (new (GetSlotPtr(world), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, HashSetPtrEnumerator<T>> GetPtrEnumerable(World world)
		{
			return new (new (GetSlotPtr(world), LastIndex));
		}
	}
}
