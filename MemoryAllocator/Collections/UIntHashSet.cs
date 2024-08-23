using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(UIntHashSetProxy))]
	public unsafe struct UIntHashSet : IIsCreated
	{
		/*public struct Enumerator
		{
			private int lastIndex;
			private int index;
			private uint current;
			private Slot* slotsPtr;

			[INLINE(256)]
			internal Enumerator(in UIntHashSet set, State* state)
			{
				lastIndex = set.lastIndex;
				index = 0;
				slotsPtr = (Slot*)set.slots.GetUnsafePtrCached(in state->allocator);
				current = default;
			}

			[INLINE(256)]
			internal Enumerator(in UIntHashSet set, Allocator allocator)
			{
				lastIndex = set.lastIndex;
				index = 0;
				slotsPtr = (Slot*)set.slots.GetUnsafePtrCached(in allocator);
				current = default;
			}

			[INLINE(256)]
			public Enumerator(Slot* slotsPtr, int lastIndex)
			{
				this.lastIndex = lastIndex;
				index = 0;
				this.slotsPtr = slotsPtr;
				current = default;
			}

			[INLINE(256)]
			public bool MoveNext()
			{
				while (index < lastIndex)
				{
					var v = slotsPtr + index;
					if (v->hashCode >= 0)
					{
						current = v->value;
						++index;
						return true;
					}

					++index;
				}

				index = lastIndex + 1;
				current = default;
				return false;
			}

			public uint Current => current;
		}*/

		public struct Slot
		{
			internal int hashCode; // Lower 31 bits of hash code, -1 if unused
			internal int next; // Index of next entry, -1 if last
			internal uint value;
		}

		public const int LOWER31_BIT_MASK = 0x7FFFFFFF;

		internal MemArray<int> buckets;
		internal MemArray<Slot> slots;
		internal int count;
		internal int lastIndex;
		internal int freeList;
		internal int version;
		public uint hash;

		public bool IsCreated
		{
			[INLINE(256)] get => buckets.IsCreated;
		}

		public uint Count
		{
			[INLINE(256)] get => (uint)count;
		}

		[INLINE(256)]
		public UIntHashSet(ref Allocator allocator, uint capacity)
		{
			this = default;
			Initialize(ref allocator, capacity);
		}

		[INLINE(256)]
		public UIntHashSet(ref Allocator allocator, in UIntHashSet other)
		{
			E.IS_CREATED(other);

			this = other;
			buckets = new MemArray<int>(ref allocator, other.buckets);
			slots = new MemArray<Slot>(ref allocator, other.slots);
		}

		[INLINE(256)]
		public bool Equals(in Allocator allocator, in UIntHashSet other)
		{
			E.IS_CREATED(this);
			E.IS_CREATED(other);

			if (count != other.count) return false;
			if (hash != other.hash) return false;
			if (count == 0u && other.count == 0u) return true;

			var slotsPtr = (Slot*)slots.GetUnsafePtr(in allocator);
			var otherSlotsPtr = (Slot*)other.slots.GetUnsafePtr(in allocator);
			var otherBucketsPtr = (int*)other.buckets.GetUnsafePtr(in allocator);
			var idx = 0u;
			while (idx < lastIndex)
			{
				var v = slotsPtr + idx;
				if (v->hashCode >= 0)
				{
					if (other.Contains(v->value, otherSlotsPtr, otherBucketsPtr) == false)
					{
						return false;
					}
				}

				++idx;
			}

			return true;
		}

		[INLINE(256)]
		public void Set(ref Allocator allocator, in UIntHashSet other)
		{
			this = other;
			buckets = new MemArray<int>(ref allocator, other.buckets);
			slots = new MemArray<Slot>(ref allocator, other.slots);
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
			buckets.BurstMode(in allocator, state);
			slots.BurstMode(in allocator, state);
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			buckets.Dispose(ref allocator);
			slots.Dispose(ref allocator);
			this = default;
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			E.IS_CREATED(this);
			return buckets.cachedPtr.memPtr;
		}

		[INLINE(256)]
		public void ReplaceWith(ref Allocator allocator, in UIntHashSet other)
		{
			if (GetMemPtr() == other.GetMemPtr())
			{
				return;
			}

			Dispose(ref allocator);
			this = other;
		}

		/*[INLINE(256)]
		public readonly Enumerator GetEnumerator(World world)
		{
			return new Enumerator(this, world.state);
		}

		[INLINE(256)]
		public readonly Enumerator GetEnumerator(State* state)
		{
			return new Enumerator(this, state);
		}

		[INLINE(256)]
		public readonly Enumerator GetEnumerator(in Allocator allocator)
		{
			return new Enumerator(this, allocator);
		}*/

		/// <summary>
		/// Remove all items from this set. This clears the elements but not the underlying
		/// buckets and slots array. Follow this call by TrimExcess to release these.
		/// </summary>
		/// <param name="allocator"></param>
		[INLINE(256)]
		public void Clear(ref Allocator allocator)
		{
			if (lastIndex > 0)
			{
				// clear the elements so that the gc can reclaim the references.
				// clear only up to m_lastIndex for m_slots
				slots.Clear(ref allocator, 0, (uint)lastIndex);
				buckets.Clear(ref allocator, 0, buckets.Length);
				lastIndex = 0;
				count = 0;
				freeList = -1;
				hash = 0u;
			}

			version++;
		}

		/// <summary>
		/// Checks if this hashset contains the item
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="item">item to check for containment</param>
		/// <returns>true if item contained; false if not</returns>
		[INLINE(256)]
		public readonly bool Contains(in Allocator allocator, uint item)
		{
			if (buckets.IsCreated)
			{
				var hashCode = item.GetHashCode() & LOWER31_BIT_MASK;
				var bucketsPtr = (int*)buckets.GetUnsafePtr(in allocator);
				var slotsPtr = (Slot*)slots.GetUnsafePtr(in allocator);
				// see note at "HashSet" level describing why "- 1" appears in for loop
				for (var i = bucketsPtr[hashCode % buckets.Length] - 1; i >= 0; i = slotsPtr[i].next)
				{
					if (slotsPtr[i].hashCode == hashCode && slotsPtr[i].value == item)
					{
						return true;
					}
				}
			}

			// either buckets is null or wasn't found
			return false;
		}

		[INLINE(256)]
		public readonly bool Contains(uint item, Slot* slotsPtr, int* bucketsPtr)
		{
			var hashCode = item & LOWER31_BIT_MASK;
			for (var i = bucketsPtr[hashCode % buckets.Length] - 1; i >= 0; i = (slotsPtr + i)->next)
			{
				if ((slotsPtr + i)->hashCode == hashCode &&
				    (slotsPtr + i)->value == item)
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public readonly bool Contains(uint item, uint hashCode, Slot* slotsPtr, int* bucketsPtr)
		{
			for (var i = bucketsPtr[hashCode % buckets.Length] - 1; i >= 0; i = (slotsPtr + i)->next)
			{
				if ((slotsPtr + i)->hashCode == hashCode &&
				    (slotsPtr + i)->value == item)
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public void RemoveExcept(ref Allocator allocator, in UIntHashSet other)
		{
			var slotsPtr = (Slot*)slots.GetUnsafePtr(in allocator);
			for (var i = 0; i < lastIndex; i++)
			{
				var slot = (slotsPtr + i);
				if (slot->hashCode >= 0)
				{
					var item = slot->value;
					if (!other.Contains(in allocator, item))
					{
						Remove(ref allocator, item);
						if (count == 0) return;
					}
				}
			}
		}

		[INLINE(256)]
		public void Remove(ref Allocator allocator, in UIntHashSet other)
		{
			var slotsPtr = (Slot*)slots.GetUnsafePtr(in allocator);
			for (var i = 0; i < lastIndex; i++)
			{
				var slot = (slotsPtr + i);
				if (slot->hashCode >= 0)
				{
					var item = slot->value;
					if (other.Contains(in allocator, item))
					{
						Remove(ref allocator, item);
						if (count == 0) return;
					}
				}
			}
		}

		[INLINE(256)]
		public void Add(ref Allocator allocator, in UIntHashSet other)
		{
			var slotsPtr = (Slot*)other.slots.GetUnsafePtr(in allocator);
			for (var i = 0; i < other.lastIndex; i++)
			{
				var slot = (slotsPtr + i);
				if (slot->hashCode >= 0)
				{
					Add(ref allocator, slot->value);
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
		public bool Remove(ref Allocator allocator, uint item)
		{
			if (this.buckets.IsCreated)
			{
				var hashCode = item.GetHashCode() & LOWER31_BIT_MASK;
				var bucket = hashCode % (int)this.buckets.Length;
				var last = -1;
				var buckets = (int*)this.buckets.GetUnsafePtr(in allocator);
				var slots = (Slot*)this.slots.GetUnsafePtr(in allocator);
				for (var i = buckets[bucket] - 1; i >= 0; last = i, i = slots[i].next)
				{
					if (slots[i].hashCode == hashCode && slots[i].value == item)
					{
						if (last < 0)
						{
							// first iteration; update buckets
							buckets[bucket] = slots[i].next + 1;
						}
						else
						{
							// subsequent iterations; update 'next' pointers
							slots[last].next = slots[i].next;
						}

						slots[i].hashCode = -1;
						slots[i].value = default;
						slots[i].next = freeList;

						hash ^= item;
						count--;
						version++;
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
			}

			// either m_buckets is null or wasn't found
			return false;
		}

		/// <summary>
		/// Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
		/// greater than or equal to capacity.
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="capacity"></param>
		[INLINE(256)]
		private void Initialize(ref Allocator allocator, uint capacity)
		{
			var size = HashHelpers.GetPrime(capacity);
			buckets = new MemArray<int>(ref allocator, size);
			slots = new MemArray<Slot>(ref allocator, size);
			freeList = -1;
		}

		/// <summary>
		/// Expand to new capacity. New capacity is next prime greater than or equal to suggested
		/// size. This is called when the underlying array is filled. This performs no
		/// defragmentation, allowing faster execution; note that this is reasonable since
		/// AddIfNotPresent attempts to insert new elements in re-opened spots.
		/// </summary>
		/// <param name="allocator"></param>
		[INLINE(256)]
		private void IncreaseCapacity(ref Allocator allocator)
		{
			var newSize = HashHelpers.ExpandPrime((uint)count);
			if (newSize <= count)
			{
				throw new System.ArgumentException();
			}

			// Able to increase capacity; copy elements to larger array and rehash
			SetCapacity(ref allocator, newSize, false);
		}

		/// <summary>
		/// Set the underlying buckets array to size newSize and rehash.  Note that newSize
		/// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
		/// instead of this method.
		/// </summary>
		[INLINE(256)]
		private void SetCapacity(ref Allocator allocator, uint newSize, bool forceNewHashCodes)
		{
			var newSlots = new MemArray<Slot>(ref allocator, newSize);
			if (slots.IsCreated)
			{
				NativeArrayUtils.CopyNoChecks(ref allocator, in slots, 0, ref newSlots, 0, (uint)lastIndex);
			}

			if (forceNewHashCodes)
			{
				for (var i = 0; i < lastIndex; i++)
				{
					if (newSlots[in allocator, i].hashCode != -1)
					{
						newSlots[in allocator, i].hashCode =
							newSlots[in allocator, i].value.GetHashCode() & LOWER31_BIT_MASK;
					}
				}
			}

			var newBuckets = new MemArray<int>(ref allocator, newSize);
			for (var i = 0; i < lastIndex; ++i)
			{
				var bucket = (uint)(newSlots[in allocator, i].hashCode % newSize);
				newSlots[in allocator, i].next = (int)newBuckets[in allocator, bucket] - 1;
				newBuckets[in allocator, bucket] = i + 1;
			}

			if (slots.IsCreated) slots.Dispose(ref allocator);
			if (buckets.IsCreated) buckets.Dispose(ref allocator);
			slots = newSlots;
			buckets = newBuckets;
		}

		/// <summary>
		/// Add item to this HashSet. Returns bool indicating whether item was added (won't be
		/// added if already present)
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="value"></param>
		/// <returns>true if added, false if already present</returns>
		[INLINE(256)]
		public bool Add(ref Allocator allocator, uint value)
		{
			var buckets = (int*)this.buckets.GetUnsafePtr(in allocator);
			var slots = (Slot*)this.slots.GetUnsafePtr(in allocator);
			return Add(ref allocator, value, ref buckets, ref slots);
		}

		[INLINE(256)]
		public bool Add(ref Allocator allocator, uint value, ref int* buckets, ref Slot* slots)
		{
			if (this.buckets.IsCreated == false)
			{
				Initialize(ref allocator, 0);
			}

			var hashCode = value.GetHashCode() & LOWER31_BIT_MASK;
			var bucket = hashCode % (int)this.buckets.Length;
			for (var i = buckets[hashCode % this.buckets.Length] - 1; i >= 0; i = slots[i].next)
			{
				if (slots[i].hashCode == hashCode && slots[i].value == value)
				{
					return false;
				}
			}

			hash ^= value;

			int index;
			if (freeList >= 0)
			{
				index = freeList;
				freeList = slots[index].next;
			}
			else
			{
				if (lastIndex == this.slots.Length)
				{
					IncreaseCapacity(ref allocator);
					// this will change during resize
					bucket = hashCode % (int)this.buckets.Length;
					buckets = (int*)this.buckets.GetUnsafePtr(in allocator);
					slots = (Slot*)this.slots.GetUnsafePtr(in allocator);
				}

				index = lastIndex;
				++lastIndex;
			}

			slots[index].hashCode = hashCode;
			slots[index].value = value;
			slots[index].next = buckets[bucket] - 1;
			buckets[bucket] = index + 1;
			++count;
			++version;

			return true;
		}

		[INLINE(256)]
		public void CopyFrom(ref Allocator allocator, in UIntHashSet other)
		{
			NativeArrayUtils.CopyExact(ref allocator, in other.buckets, ref buckets);
			slots.CopyFrom(ref allocator, other.slots);
			var thisBuckets = buckets;
			var thisSlots = slots;
			this = other;
			buckets = thisBuckets;
			slots = thisSlots;
		}

		public uint GetReservedSizeInBytes()
		{
			return buckets.GetReservedSizeInBytes() + slots.GetReservedSizeInBytes();
		}
	}
}
