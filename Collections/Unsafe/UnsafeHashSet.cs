using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.Collections
{
	[DebuggerTypeProxy(typeof(UnsafeHashSet<>.HashSetProxy))]
	public struct UnsafeHashSet<T> : IDisposable
		where T : unmanaged, IEquatable<T>
	{
		public struct Slot
		{
			internal int hashCode; // Lower 31 bits of hash code, -1 if unused
			internal int next; // Index of next entry, -1 if last
			public T value;
		}

		private const int _hashCodeMask = 0x7FFFFFFF;

		internal UnsafeArray<int> buckets;
		internal UnsafeArray<Slot> slots;
		internal int count;
		internal int lastIndex;
		internal int freeList;
		internal int hash;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => buckets.IsCreated;
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => count;
		}

		public int LastIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => lastIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UnsafeHashSet(int capacity = 8) : this(default, capacity)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UnsafeHashSet(Id<MemoryManager> memoryId, int capacity)
		{
			this = default;
			Initialize(memoryId, capacity);
		}

		/// <summary>
		/// Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
		/// greater than or equal to capacity.
		/// </summary>
		/// <param name="memoryId"></param>
		/// <param name="capacity"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Initialize(Id<MemoryManager> memoryId, int capacity)
		{
			var prime = capacity.GetPrime();
			buckets = new UnsafeArray<int>(memoryId, prime);
			slots = new UnsafeArray<Slot>(memoryId, prime);
			freeList = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<Slot> GetSlotPtr()
		{
			E.ASSERT(IsCreated);
			return slots.ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			buckets.Dispose();
			slots.Dispose();
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReplaceWith(in UnsafeHashSet<T> other)
		{
			if (GetSlotPtr() == other.GetSlotPtr())
				return;

			Dispose();
			this = other;
		}

		/// <summary>
		/// Remove all items from this set. This clears the elements but not the underlying
		/// buckets and slots array. Follow this call by TrimExcess to release these.
		/// </summary>
		/// <param name="worldState"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			if (lastIndex > 0)
			{
				// clear the elements so that the gc can reclaim the references.
				// clear only up to m_lastIndex for m_slots
				slots.Clear(0, lastIndex);
				buckets.Clear(0, buckets.Length);
				lastIndex = 0;
				count = 0;
				freeList = -1;
				hash = 0;
			}
		}

		/// <summary>
		/// Checks if this hashset contains the item
		/// </summary>
		/// <param name="worldState"></param>
		/// <param name="item">item to check for containment</param>
		/// <returns>true if item contained; false if not</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(in T item)
		{
			return Contains(item, slots.ptr, buckets.ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(ref UnsafeHashSet<T> other)
		{
			var slotsPtr = slots.ptr;
			for (var i = 0; i < other.lastIndex; i++)
			{
				ref var slot = ref (slotsPtr + i).Value();
				if (slot.hashCode >= 0)
				{
					Add(slot.value);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveExcept(ref UnsafeHashSet<T> other)
		{
			var slotsPtr = slots.ptr;
			for (var i = 0; i < lastIndex; i++)
			{
				ref var slot = ref slotsPtr[i];
				if (slot.hashCode >= 0)
				{
					var item = slot.value;
					if (!other.Contains(item))
					{
						Remove(item);
						if (count == 0)
							return;
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(ref UnsafeHashSet<T> other)
		{
			E.ASSERT(IsCreated);

			var slotsPtr = slots.ptr;
			for (var i = 0; i < lastIndex; i++)
			{
				ref var slot = ref (slotsPtr + i).Value();
				if (slot.hashCode >= 0)
				{
					var item = slot.value;
					if (other.Contains(item))
					{
						Remove(item);
						if (count == 0)
							return;
					}
				}
			}
		}

		/// <summary>
		/// Remove item from this hashset
		/// </summary>
		/// <param name="item">item to remove</param>
		/// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(in T item)
		{
			if (!buckets.IsCreated)
				return false;

			var slotsPtr = slots.ptr;
			var bucketsPtr = buckets.ptr;

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
		/// <param name="worldState"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncreaseCapacity()
		{
			var newSize = count.ExpandPrime();

			// Able to increase capacity; copy elements to larger array and rehash
			SetCapacity(newSize);
		}

		/// <summary>
		/// Set the underlying buckets array to size newSize and rehash.  Note that newSize
		/// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
		/// instead of this method.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetCapacity(int newSize)
		{
			var newSlots = new UnsafeArray<Slot>(slots.memoryId, newSize, ClearOptions.ClearMemory);
			if (slots.IsCreated)
			{
				MemoryExt.MemCopy(slots.ptr, newSlots.ptr, count);
			}

			var newBuckets = new UnsafeArray<int>(buckets.memoryId, newSize, ClearOptions.ClearMemory);
			for (var i = 0; i < lastIndex; i++)
			{
				var bucket = newSlots[i].hashCode % newSize;
				newSlots[i].next = newBuckets[bucket] - 1;
				newBuckets[bucket] = i + 1;
			}

			if (slots.IsCreated)
				slots.Dispose();
			if (buckets.IsCreated)
				buckets.Dispose();
			slots = newSlots;
			buckets = newBuckets;
		}

		/// <summary>
		/// Add item to this HashSet. Returns bool indicating whether item was added (won't be
		/// added if already present)
		/// </summary>
		/// <param name="worldState"></param>
		/// <param name="value"></param>
		/// <returns>true if added, false if already present</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Add(in T value)
		{
			if (!buckets.IsCreated)
			{
				Initialize(default, 0);
			}

			var bucketsPtr = buckets.ptr;
			var slotsPtr = slots.ptr;

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
					IncreaseCapacity();
					// this will change during resize
					bucket = hashCode % buckets.Length;

					bucketsPtr = buckets.ptr;
					slotsPtr = slots.ptr;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int GetHash()
		{
			return hash;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(in UnsafeHashSet<T> other)
		{
			buckets.CopyFrom(other.buckets);

			if (slots.Length >= other.slots.Length)
				MemoryExt.MemCopy(other.slots.ptr, slots.ptr, count);
			else
				slots.CopyFrom(other.slots);

			var thisBuckets = buckets;
			var thisSlots = slots;
			this = other;

			buckets = thisBuckets;
			slots = thisSlots;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator GetEnumerator()
		{
			return new Enumerator(GetSlotPtr(), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SlotEnumerator GetSlotEnumerator()
		{
			return new SlotEnumerator(GetSlotPtr(), LastIndex);
		}

		private class HashSetProxy
		{
			private UnsafeHashSet<T> _hashSet;

			public HashSetProxy(UnsafeHashSet<T> hashSet)
			{
				_hashSet = hashSet;
			}

			public UnsafeArray<int> Buckets => _hashSet.buckets;
			public UnsafeArray<UnsafeHashSet<T>.Slot> Slots => _hashSet.slots;
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
					foreach (var value in _hashSet)
					{
						arr[i++] = value;
					}

					return arr;
#else
					return Array.Empty<T>();
#endif
				}
			}
		}

		public ref struct Enumerator
		{
			private SlotEnumerator _slotEnumerator;

			internal Enumerator(SafePtr<Slot> entries, int count)
			{
				_slotEnumerator = new SlotEnumerator(entries, count);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				return _slotEnumerator.MoveNext();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				_slotEnumerator.Reset();
			}

			public ref T Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => ref _slotEnumerator.Current.value;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				_slotEnumerator.Dispose();
			}
		}

		public ref struct SlotEnumerator
		{
			private readonly SafePtr<Slot> _entries;
			private readonly int _count;
			private int _index;

			internal SlotEnumerator(SafePtr<Slot> entries, int count)
			{
				_entries = entries;
				_count = count;
				_index = -1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				while (++_index < _count)
				{
					if (_entries[_index].hashCode >= 0)
						return true;
				}

				_index = _count;
				return false;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				_index = -1;
			}

			public ref Slot Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => ref (_entries + _index).Value();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				this = default;
			}
		}
	}
}
