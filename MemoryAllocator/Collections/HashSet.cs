using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public unsafe struct HashSet<T> : IIsCreated, IHashSetEnumerable<T> where T : unmanaged
	{
		public struct Slot
		{
			internal int hashCode; // Lower 31 bits of hash code, -1 if unused
			internal int next; // Index of next entry, -1 if last
			internal T value;
		}

		public const int lower31BITMask = 0x7FFFFFFF;

		internal MemArray<int> buckets;
		internal MemArray<Slot> slots;
		internal int count;
		internal int lastIndex;
		internal int freeList;
		internal int hash;

		public bool IsCreated
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
		public HashSet(Allocator* allocator, int capacity)
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

		/// <summary>
		/// Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
		/// greater than or equal to capacity.
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="capacity"></param>
		[INLINE(256)]
		private void Initialize(Allocator* allocator, int capacity)
		{
			var size = HashHelpers.GetPrime(capacity);
			buckets = new MemArray<int>(allocator, size);
			this.slots = new MemArray<Slot>(allocator, size);
			var slots = (Slot*)this.slots.GetPtr(allocator);
			for (var i = 0; i < this.slots.Length; ++i)
			{
				(*(slots + i)).hashCode = -1;
			}

			freeList = -1;
		}

		[INLINE(256)]
		public Slot* GetSlotPtr()
		{
			return slots.GetValuePtr();
		}

		[INLINE(256)]
		public Slot* GetSlotPtr(Allocator* allocator)
		{
			return slots.GetValuePtr(allocator);
		}

		[INLINE(256)]
		public bool Equals(Allocator* allocator, ref HashSet<T> other)
		{
			Debug.Assert(IsCreated);
			Debug.Assert(other.IsCreated);

			if (count != other.count)
				return false;
			if (hash != other.hash)
				return false;
			if (count == 0u && other.count == 0u)
				return true;

			var slotsPtr = (Slot*)slots.GetPtr(allocator);
			var otherSlotsPtr = (Slot*)other.slots.GetPtr(allocator);
			var otherBucketsPtr = (int*)other.buckets.GetPtr(allocator);
			var idx = 0u;
			while (idx < lastIndex)
			{
				var v = slotsPtr + idx;
				if (v->hashCode >= 0)
				{
					if (!other.Contains(v->value, otherSlotsPtr, otherBucketsPtr))
					{
						return false;
					}
				}

				++idx;
			}

			return true;
		}

		[INLINE(256)]
		public void Set(Allocator* allocator, in HashSet<T> other)
		{
			this = other;
			buckets = new MemArray<int>(allocator, other.buckets);
			slots = new MemArray<Slot>(allocator, other.slots);
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
		private bool Contains(in T item, Slot* slotsPtr, int* bucketsPtr)
		{
			Debug.Assert(IsCreated);
			// see note at "HashSet" level describing why "- 1" appears in for loop
			var hashCode = GetHashCode(item) & lower31BITMask;
			for (var i = bucketsPtr[hashCode % buckets.Length] - 1; i >= 0; i = (slotsPtr + i)->next)
			{
				if ((slotsPtr + i)->hashCode == hashCode && Equal((slotsPtr + i)->value, item))
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
			var slotsPtr = (Slot*)slots.GetPtr(allocator);
			for (var i = 0; i < lastIndex; i++)
			{
				var slot = (slotsPtr + i);
				if (slot->hashCode >= 0)
				{
					var item = slot->value;
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
			var slotsPtr = (Slot*)slots.GetPtr(allocator);
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
			var slotsPtr = (Slot*)other.slots.GetPtr(allocator);
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

			var hashCode = GetHashCode(item) & lower31BITMask;
			var bucket = hashCode % buckets.Length;
			var last = -1;
			var bucketsPtr = (int*)buckets.GetPtr(allocator);
			var slotsPtr = (Slot*)slots.GetPtr(allocator);
			for (var i = *(bucketsPtr + bucket) - 1; i >= 0; last = i, i = (slotsPtr + i)->next)
			{
				var slot = slotsPtr + i;
				if (slot->hashCode == hashCode && Equal(slot->value, item))
				{
					if (last < 0)
					{
						// first iteration; update buckets
						*(bucketsPtr + bucket) = slot->next + 1;
					}
					else
					{
						// subsequent iterations; update 'next' pointers
						(slotsPtr + last)->next = slot->next;
					}

					slot->hashCode = -1;
					slot->value = default;
					slot->next = freeList;

					hash ^= GetHashCode(item);

					if (--count == 0)
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
			var newSize = HashHelpers.ExpandPrime(count);

			// Able to increase capacity; copy elements to larger array and rehash
			SetCapacity(allocator, newSize, false);
		}

		/// <summary>
		/// Set the underlying buckets array to size newSize and rehash.  Note that newSize
		/// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
		/// instead of this method.
		/// </summary>
		[INLINE(256)]
		private void SetCapacity(Allocator* allocator, int newSize, bool forceNewHashCodes)
		{
			var newSlots = new MemArray<Slot>(allocator, newSize);
			if (slots.IsCreated)
			{
				MemArrayExt.CopyNoChecks(allocator, slots, 0, ref newSlots, 0, lastIndex);
			}

			if (forceNewHashCodes)
			{
				for (var i = 0; i < lastIndex; i++)
				{
					if (newSlots[allocator, i].hashCode != -1)
					{
						newSlots[allocator, i].hashCode = GetHashCode(newSlots[allocator, i].value);
					}
				}
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

			var hashCode = GetHashCode(value) & lower31BITMask;
			var bucket = hashCode % buckets.Length;
			for (var i = *(bucketsPtr + bucket) - 1; i >= 0; i = (slotsPtr + i)->next)
			{
				var slot = slotsPtr + i;
				if (slot->hashCode == hashCode && Equal(slot->value, value))
				{
					return false;
				}
			}

			hash ^= GetHashCode(value);

			int index;
			if (freeList >= 0)
			{
				index = freeList;
				freeList = (slotsPtr + index)->next;
			}
			else
			{
				if (lastIndex == slots.Length)
				{
					IncreaseCapacity(allocator);
					// this will change during resize
					bucketsPtr = buckets.GetValuePtr(allocator);
					slotsPtr = slots.GetValuePtr(allocator);
					bucket = hashCode % buckets.Length;
				}

				index = lastIndex;
				++lastIndex;
			}

			{
				var slot = slotsPtr + index;
				slot->hashCode = hashCode;
				slot->value = value;
				slot->next = *(bucketsPtr + bucket) - 1;
				*(bucketsPtr + bucket) = (int)(index + 1u);
				++count;
			}

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
		public static int GetHashCode(T item)
		{
			return EqualityComparer<T>.Default.GetHashCode(item);
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
		public Enumerable<T, HashSetEnumerator<T>> GetEnumerable(Allocator* allocator)
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
