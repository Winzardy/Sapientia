using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
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

		[INLINE(256)]
		public Allocator* GetAllocatorPtr()
		{
			return buckets.GetAllocatorPtr();
		}

		[INLINE(256)]
		public HashSet(Allocator* allocator, int capacity = 8)
		{
			this = default;
			Initialize(allocator, capacity);
		}

		[INLINE(256)]
		public HashSet(Allocator* allocator, in HashSet<T> other)
		{
			Debug.Assert(other.IsCreated);

			this = other;
			buckets = new MemArray<int>(allocator, other.buckets);
			slots = new MemArray<Slot>(allocator, other.slots);
		}

		[INLINE(256)]
		public HashSet(Allocator* allocator, in ICollection<T> other) : this(allocator, other.Count)
		{
			foreach (var value in other)
			{
				Add(allocator, value);
			}
		}

		[INLINE(256)]
		public HashSet(Allocator* allocator, in IEnumerable<T> other, int capacity) : this(allocator, capacity)
		{
			foreach (var value in other)
			{
				Add(allocator, value);
			}
		}

		/// <summary>
		/// Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
		/// greater than or equal to capacity.
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="capacity"></param>
		[INLINE(256)]
		private void Initialize(Allocator* allocator, int capacity)
		{
			var size = capacity.GetPrime();
			buckets = new MemArray<int>(allocator, size);
			slots = new MemArray<Slot>(allocator, size);

			var rawSlots = (Slot*)slots.GetPtr(allocator);
			for (var i = 0; i < slots.Length; ++i)
			{
				(*(rawSlots + i)).hashCode = -1;
			}

			freeList = -1;
		}

		[INLINE(256)]
		public Slot* GetSlotPtr()
		{
			E.ASSERT(IsCreated);
			return slots.GetValuePtr();
		}

		[INLINE(256)]
		public Slot* GetSlotPtr(Allocator* allocator)
		{
			E.ASSERT(IsCreated);
			return slots.GetValuePtr(allocator);
		}

		[INLINE(256)]
		public void Dispose(Allocator* allocator)
		{
			buckets.Dispose(allocator);
			slots.Dispose(allocator);
			this = default;
		}

		[INLINE(256)]
		public void Dispose()
		{
			Dispose(GetAllocatorPtr());
		}

		[INLINE(256)]
		public MemPtr GetMemPtr()
		{
			E.ASSERT(IsCreated);
			return buckets.innerArray.ptr.memPtr;
		}

		[INLINE(256)]
		public void ReplaceWith(Allocator* allocator, ref HashSet<T> other)
		{
			if (GetMemPtr() == other.GetMemPtr())
			{
				return;
			}

			Dispose(allocator);
			this = other;
		}

		/// <summary>
		/// Remove all items from this set. This clears the elements but not the underlying
		/// buckets and slots array. Follow this call by TrimExcess to release these.
		/// </summary>
		/// <param name="allocator"></param>
		[INLINE(256)]
		public void Clear(Allocator* allocator)
		{
			if (lastIndex > 0)
			{
				// clear the elements so that the gc can reclaim the references.
				// clear only up to m_lastIndex for m_slots
				slots.Clear(allocator, 0, lastIndex);
				buckets.Clear(allocator, 0, buckets.Length);
				lastIndex = 0;
				count = 0;
				freeList = -1;
				hash = 0;
			}
		}

		/// <summary>
		/// Checks if this hashset contains the item
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="item">item to check for containment</param>
		/// <returns>true if item contained; false if not</returns>
		[INLINE(256)]
		public bool Contains(Allocator* allocator, in T item)
		{
			return Contains(item, slots.GetValuePtr(allocator), buckets.GetValuePtr(allocator));
		}

		[INLINE(256)]
		private readonly bool Contains(in T item, Slot* slotsPtr, int* bucketsPtr)
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
		public void RemoveExcept(Allocator* allocator, ref HashSet<T> other)
		{
			var slotsPtr = slots.GetValuePtr(allocator);
			for (var i = 0; i < lastIndex; i++)
			{
				ref var slot = ref slotsPtr[i];
				if (slot.hashCode >= 0)
				{
					var item = slot.value;
					if (!other.Contains(allocator, item))
					{
						Remove(allocator, item);
						if (count == 0)
							return;
					}
				}
			}
		}

		[INLINE(256)]
		public void Remove(Allocator* allocator, ref HashSet<T> other)
		{
			E.ASSERT(IsCreated);

			var slotsPtr = slots.GetValuePtr(allocator);
			for (var i = 0; i < lastIndex; i++)
			{
				var slot = (slotsPtr + i);
				if (slot->hashCode >= 0)
				{
					var item = slot->value;
					if (other.Contains(allocator, item))
					{
						Remove(allocator, item);
						if (count == 0)
							return;
					}
				}
			}
		}

		[INLINE(256)]
		public void Add(Allocator* allocator, ref HashSet<T> other)
		{
			var slotsPtr = slots.GetValuePtr(allocator);
			for (var i = 0; i < other.lastIndex; i++)
			{
				var slot = (slotsPtr + i);
				if (slot->hashCode >= 0)
				{
					Add(allocator, slot->value);
				}
			}
		}

		/// <summary>
		/// Remove item from this hashset
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="item">item to remove</param>
		/// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
		[INLINE(256)]
		public bool Remove(in T item)
		{
			return Remove(GetAllocatorPtr(), item);
		}

		/// <summary>
		/// Remove item from this hashset
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="item">item to remove</param>
		/// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
		[INLINE(256)]
		public bool Remove(Allocator* allocator, in T item)
		{
			if (!buckets.IsCreated)
				return false;

			var slotsPtr = slots.GetValuePtr(allocator);
			var bucketsPtr = buckets.GetValuePtr(allocator);

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
		/// <param name="allocator"></param>
		[INLINE(256)]
		private void IncreaseCapacity(Allocator* allocator)
		{
			var newSize = count.ExpandPrime();

			// Able to increase capacity; copy elements to larger array and rehash
			SetCapacity(allocator, newSize);
		}

		/// <summary>
		/// Set the underlying buckets array to size newSize and rehash.  Note that newSize
		/// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
		/// instead of this method.
		/// </summary>
		[INLINE(256)]
		private void SetCapacity(Allocator* allocator, int newSize)
		{
			var newSlots = new MemArray<Slot>(allocator, newSize);
			if (slots.IsCreated)
			{
				MemArrayExt.CopyNoChecks(allocator, slots, 0, ref newSlots, 0, lastIndex);
			}

			var newBuckets = new MemArray<int>(allocator, newSize);
			for (var i = 0; i < lastIndex; ++i)
			{
				var bucket = newSlots[allocator, i].hashCode % newSize;
				newSlots[allocator, i].next = newBuckets[allocator, bucket] - 1;
				newBuckets[allocator, bucket] = i + 1;
			}

			if (slots.IsCreated)
				slots.Dispose(allocator);
			if (buckets.IsCreated)
				buckets.Dispose(allocator);
			slots = newSlots;
			buckets = newBuckets;
		}


		/// <summary>
		/// Add item to this HashSet. Returns bool indicating whether item was added (won't be
		/// added if already present)
		/// </summary>
		/// <param name="value"></param>
		/// <returns>true if added, false if already present</returns>
		[INLINE(256)]
		public bool Add(in T value)
		{
			return Add(GetAllocatorPtr(), value);
		}

		/// <summary>
		/// Add item to this HashSet. Returns bool indicating whether item was added (won't be
		/// added if already present)
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="value"></param>
		/// <returns>true if added, false if already present</returns>
		[INLINE(256)]
		public bool Add(Allocator* allocator, in T value)
		{
			if (!buckets.IsCreated)
			{
				Initialize(allocator, 0);
			}

			var bucketsPtr = buckets.GetValuePtr(allocator);
			var slotsPtr = slots.GetValuePtr(allocator);

			var hashCode = value.GetHashCode() & _hashCodeMask;
			var bucket = hashCode % buckets.Length;

			for (var i = bucketsPtr[hashCode % buckets.Length] - 1; i >= 0; i = slotsPtr[i].next)
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
					IncreaseCapacity(allocator);
					// this will change during resize
					bucket = hashCode % buckets.Length;
				}
				index = lastIndex;
				lastIndex++;
			}
			slots[index].hashCode = hashCode;
			slots[index].value = value;
			slots[index].next = buckets[bucket] - 1;
			buckets[bucket] = index + 1;
			count++;

			return true;
		}

		[INLINE(256)]
		public readonly int GetHash()
		{
			return hash;
		}

		[INLINE(256)]
		public void CopyFrom(Allocator* allocator, in HashSet<T> other)
		{
			MemArrayExt.CopyExact(allocator, in other.buckets, ref buckets);
			slots.CopyFrom(allocator, other.slots);
			var thisBuckets = buckets;
			var thisSlots = slots;
			this = other;
			buckets = thisBuckets;
			slots = thisSlots;
		}

		[INLINE(256)]
		public static bool Equal(T v1, T v2)
		{
			return v1.IsEquals<T>(ref v2);
			//return EqualityComparer<T>.Default.Equals(v1, v2);
		}

		[INLINE(256)]
		public HashSetEnumerator<T> GetEnumerator(Allocator* allocator)
		{
			return new HashSetEnumerator<T>(GetSlotPtr(allocator), LastIndex);
		}

		[INLINE(256)]
		public HashSetEnumerator<T> GetEnumerator()
		{
			return new HashSetEnumerator<T>(GetSlotPtr(), LastIndex);
		}

		[INLINE(256)]
		public HashSetPtrEnumerator<T> GetPtrEnumerator(Allocator* allocator)
		{
			return new HashSetPtrEnumerator<T>(GetSlotPtr(allocator), LastIndex);
		}

		[INLINE(256)]
		public HashSetPtrEnumerator<T> GetPtrEnumerator()
		{
			return new HashSetPtrEnumerator<T>(GetSlotPtr(), LastIndex);
		}

		[INLINE(256)]
		public readonly Enumerable<T, HashSetEnumerator<T>> GetEnumerable(Allocator* allocator)
		{
			return new (new (GetSlotPtr(allocator), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<T, HashSetEnumerator<T>> GetEnumerable()
		{
			return new (new (GetSlotPtr(), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, HashSetPtrEnumerator<T>> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new (GetSlotPtr(allocator), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, HashSetPtrEnumerator<T>> GetPtrEnumerable()
		{
			return new (new (GetSlotPtr(), LastIndex));
		}
	}
}
