using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public readonly unsafe ref struct UIntPairHashSetRead
	{
		public ref struct Enumerator
		{
			internal UIntPairHashSetRead set;
			private uint index;
			private UIntPair current;

			[INLINE(256)]
			public bool MoveNext()
			{
				while (index < set.lastIndex)
				{
					var v = set.slotsPtr + index;
					if (v->hashCode >= 0)
					{
						current = v->value;
						++index;
						return true;
					}

					++index;
				}

				index = set.lastIndex + 1u;
				current = default;
				return false;
			}

			public UIntPair Current => current;
		}

		public readonly UIntPairHashSet.Slot* slotsPtr;
		public readonly int* bucketsPtr;
		public readonly uint lastIndex;
		public readonly uint hash;
		public readonly uint bucketsLength;

		[INLINE(256)]
		public UIntPairHashSetRead(in Allocator allocator, in UIntPairHashSet set)
		{
			bucketsPtr = (int*)set.buckets.GetPtr(in allocator);
			slotsPtr = (UIntPairHashSet.Slot*)set.slots.GetPtr(in allocator);
			lastIndex = set.lastIndex;
			hash = set.hash;
			bucketsLength = set.buckets.Length;
		}

		[INLINE(256)]
		public static UIntPairHashSetRead Create(in Allocator allocator, in UIntPairHashSet set)
		{
			return new UIntPairHashSetRead(in allocator, in set);
		}

		[INLINE(256)]
		public bool Contains(UIntPair item)
		{
			var hashCode = item.GetHash() & UIntPairHashSet.LOWER31_BIT_MASK;
			for (var i = bucketsPtr[hashCode % bucketsLength] - 1; i >= 0; i = (slotsPtr + i)->next)
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
		public Enumerator GetEnumerator()
		{
			Enumerator e = default;
			e.set = this;
			return e;
		}
	}

	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(UIntPairHashSetProxy))]
	public unsafe struct UIntPairHashSet : IIsCreated
	{
		/*public struct Enumerator
		{
			private uint lastIndex;
			private uint index;
			private UIntPair current;
			private Slot* slotsPtr;

			[INLINE(256)]
			internal Enumerator(in UIntPairHashSet set, State* state)
			{
				lastIndex = set.lastIndex;
				index = 0;
				slotsPtr = (Slot*)set.slots.GetUnsafePtrCached(in state->allocator);
				current = default;
			}

			[INLINE(256)]
			internal Enumerator(in UIntPairHashSet set, Allocator allocator)
			{
				lastIndex = set.lastIndex;
				index = 0;
				slotsPtr = (Slot*)set.slots.GetUnsafePtrCached(in allocator);
				current = default;
			}

			[INLINE(256)]
			public Enumerator(Slot* slotsPtr, uint lastIndex)
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

				index = lastIndex + 1u;
				current = default;
				return false;
			}

			public UIntPair Current => current;
		}*/

		public struct Slot
		{
			internal int hashCode; // Lower 31 bits of hash code, -1 if unused
			internal int next; // Index of next entry, -1 if last
			internal UIntPair value;
		}

		public const int LOWER31_BIT_MASK = 0x7FFFFFFF;

		internal MemArray<uint> buckets;
		internal MemArray<Slot> slots;
		internal uint count;
		internal uint lastIndex;
		internal int freeList;
		internal uint version;
		internal uint hash;

		public bool IsCreated
		{
			[INLINE(256)] get => buckets.IsCreated;
		}

		public uint Count
		{
			[INLINE(256)] get => count;
		}

		[INLINE(256)]
		public UIntPairHashSet(ref Allocator allocator, uint capacity)
		{
			this = default;
			Initialize(ref allocator, capacity);
		}

		[INLINE(256)]
		public UIntPairHashSet(ref Allocator allocator, in UIntPairHashSet other)
		{
			E.IS_CREATED(other);

			this = other;
			buckets = new MemArray<uint>(ref allocator, other.buckets);
			slots = new MemArray<Slot>(ref allocator, other.slots);
		}

		[INLINE(256)]
		public bool Equals(in Allocator allocator, in UIntPairHashSet other)
		{
			E.IS_CREATED(other);

			if (count != other.count) return false;
			if (hash != other.hash) return false;
			if (count == 0u && other.count == 0u) return true;

			var slotsPtr = (Slot*)slots.GetPtr(in allocator);
			var otherSlotsPtr = (Slot*)other.slots.GetPtr(in allocator);
			var otherBucketsPtr = (int*)other.buckets.GetPtr(in allocator);
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
		public void Set(ref Allocator allocator, in UIntPairHashSet other)
		{
			this = other;
			buckets = new MemArray<uint>(ref allocator, other.buckets);
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
			return buckets.innerArray.ptr.memPtr;
		}

		[INLINE(256)]
		public void ReplaceWith(ref Allocator allocator, in UIntPairHashSet other)
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
		public void Clear(in Allocator allocator)
		{
			if (lastIndex > 0)
			{
				// clear the elements so that the gc can reclaim the references.
				// clear only up to m_lastIndex for m_slots
				slots.Clear(allocator, 0, lastIndex);
				buckets.Clear(allocator, 0, buckets.Length);
				lastIndex = 0u;
				count = 0u;
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
		public readonly bool Contains(in Allocator allocator, UIntPair item)
		{
			var hashCode = item.GetHash() & LOWER31_BIT_MASK;
			// see note at "HashSet" level describing why "- 1" appears in for loop
			var slotsPtr = (Slot*)slots.GetPtr(in allocator);
			for (var i = (int)buckets[in allocator, hashCode % buckets.Length] - 1; i >= 0; i = (slotsPtr + i)->next)
			{
				if ((slotsPtr + i)->hashCode == hashCode &&
				    (slotsPtr + i)->value == item)
				{
					return true;
				}
			}

			// either m_buckets is null or wasn't found
			return false;
		}

		[INLINE(256)]
		public readonly bool Contains(UIntPair item, Slot* slotsPtr, int* bucketsPtr)
		{
			var hashCode = item.GetHash() & LOWER31_BIT_MASK;
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
		public readonly bool Contains(UIntPair item, uint hashCode, Slot* slotsPtr, int* bucketsPtr)
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
		public void RemoveExcept(ref Allocator allocator, in UIntPairHashSet other)
		{
			var slotsPtr = (Slot*)slots.GetPtr(in allocator);
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
		public void Remove(ref Allocator allocator, in UIntPairHashSet other)
		{
			var slotsPtr = (Slot*)slots.GetPtr(in allocator);
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
		public void Add(ref Allocator allocator, in UIntPairHashSet other)
		{
			var slotsPtr = (Slot*)other.slots.GetPtr(in allocator);
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
		public bool Remove(ref Allocator allocator, UIntPair item)
		{
			if (buckets.IsCreated)
			{
				var hashCode = item.GetHash() & LOWER31_BIT_MASK;
				var bucket = hashCode % buckets.Length;
				var last = -1;
				var bucketsPtr = (int*)buckets.GetPtr(in allocator);
				var slotsPtr = (Slot*)slots.GetPtr(in allocator);
				for (var i = *(bucketsPtr + bucket) - 1; i >= 0; last = i, i = (slotsPtr + i)->next)
				{
					var slot = slotsPtr + i;
					if (slot->hashCode == hashCode &&
					    slot->value == item)
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

						hash ^= item.GetHash();

						++version;
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
			buckets = new MemArray<uint>(ref allocator, size);
			this.slots = new MemArray<Slot>(ref allocator, size);
			var slots = (Slot*)this.slots.GetPtr(in allocator);
			for (var i = 0; i < this.slots.Length; ++i)
			{
				(*(slots + i)).hashCode = -1;
			}

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
			var newSize = HashHelpers.ExpandPrime(count);
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
				MemArrayExt.CopyNoChecks(ref allocator, slots, 0, ref newSlots, 0, lastIndex);
			}

			if (forceNewHashCodes)
			{
				for (var i = 0; i < lastIndex; i++)
				{
					if (newSlots[in allocator, i].hashCode != -1)
					{
						newSlots[in allocator, i].hashCode = (int)newSlots[in allocator, i].value.GetHash();
					}
				}
			}

			var newBuckets = new MemArray<uint>(ref allocator, newSize);
			for (uint i = 0; i < lastIndex; ++i)
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
		public bool Add(ref Allocator allocator, UIntPair value)
		{
			if (buckets.IsCreated == false)
			{
				Initialize(ref allocator, 0);
			}

			var bucketsPtr = (int*)buckets.GetPtr(in allocator);
			var slotsPtr = (Slot*)slots.GetPtr(in allocator);

			var hashCode = value.GetHash() & UIntHashSet.LOWER31_BIT_MASK;
			var bucket = hashCode % buckets.Length;
			for (var i = *(bucketsPtr + bucket) - 1; i >= 0; i = (slotsPtr + i)->next)
			{
				var slot = slotsPtr + i;
				if (slot->hashCode == hashCode &&
				    slot->value == value)
				{
					return false;
				}
			}

			hash ^= value.GetHash();

			uint index;
			if (freeList >= 0)
			{
				index = (uint)freeList;
				freeList = (slotsPtr + index)->next;
			}
			else
			{
				if (lastIndex == slots.Length)
				{
					IncreaseCapacity(ref allocator);
					// this will change during resize
					bucketsPtr = (int*)buckets.GetPtr(in allocator);
					slotsPtr = (Slot*)slots.GetPtr(in allocator);
					bucket = hashCode % buckets.Length;
				}

				index = lastIndex;
				++lastIndex;
			}

			{
				var slot = slotsPtr + index;
				slot->hashCode = (int)hashCode;
				slot->value = value;
				slot->next = *(bucketsPtr + bucket) - 1;
				*(bucketsPtr + bucket) = (int)(index + 1u);
				++count;
				++version;
			}

			return true;
		}

		[INLINE(256)]
		public bool Add(ref Allocator allocator, UIntPair value, ref int* bucketsPtr, ref Slot* slotsPtr)
		{
			if (buckets.IsCreated == false)
			{
				Initialize(ref allocator, 0);
			}

			var hashCode = value.GetHash() & UIntHashSet.LOWER31_BIT_MASK;
			var bucket = hashCode % buckets.Length;
			for (var i = *(bucketsPtr + bucket) - 1; i >= 0; i = (slotsPtr + i)->next)
			{
				var slot = slotsPtr + i;
				if (slot->hashCode == hashCode &&
				    slot->value == value)
				{
					return false;
				}
			}

			hash ^= value.GetHash();

			uint index;
			if (freeList >= 0)
			{
				index = (uint)freeList;
				freeList = (slotsPtr + index)->next;
			}
			else
			{
				if (lastIndex == slots.Length)
				{
					IncreaseCapacity(ref allocator);
					// this will change during resize
					bucketsPtr = (int*)buckets.GetPtr(in allocator);
					slotsPtr = (Slot*)slots.GetPtr(in allocator);
					bucket = hashCode % buckets.Length;
				}

				index = lastIndex;
				++lastIndex;
			}

			{
				var slot = slotsPtr + index;
				slot->hashCode = (int)hashCode;
				slot->value = value;
				slot->next = *(bucketsPtr + bucket) - 1;
				*(bucketsPtr + bucket) = (int)(index + 1u);
				++count;
				++version;
			}

			return true;
		}

		[INLINE(256)]
		public readonly uint GetHash()
		{
			return hash;
		}

		[INLINE(256)]
		public void CopyFrom(ref Allocator allocator, in UIntPairHashSet other)
		{
			buckets.CopyFrom(ref allocator, other.buckets);
			slots.CopyFrom(ref allocator, other.slots);
			var thisBuckets = buckets;
			var thisSlots = slots;
			this = other;
			buckets = thisBuckets;
			slots = thisSlots;
		}
	}
}
