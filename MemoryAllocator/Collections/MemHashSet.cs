using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxy(typeof(MemHashSet<>.HashSetProxy))]
	public struct MemHashSet<T>
		where T : unmanaged, IEquatable<T>
	{
		public struct Slot
		{
			internal int hashCode; // Lower 31 bits of hash code, -1 if unused
			internal int next; // Index of next entry, -1 if last
			internal T value;
		}

		private const int _hashCodeMask = 0x7FFFFFFF;

		private MemArray<int> _buckets;
		private MemArray<Slot> _slots;
		private int _count;
		private int _lastIndex;
		private int _freeList;
		private int _hash;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _buckets.IsCreated;
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _count;
		}

		public int LastIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _lastIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemHashSet(WorldState worldState, int capacity = 8)
		{
			this = default;
			Initialize(worldState, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemHashSet(WorldState worldState, in MemHashSet<T> other)
		{
			E.ASSERT(other.IsCreated);

			this = other;
			_buckets = new MemArray<int>(worldState, other._buckets);
			_slots = new MemArray<Slot>(worldState, other._slots);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemHashSet(WorldState worldState, in ICollection<T> other) : this(worldState, other.Count)
		{
			foreach (var value in other)
			{
				Add(worldState, value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemHashSet(WorldState worldState, in IEnumerable<T> other, int capacity) : this(worldState, capacity)
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
		/// <param name="worldState"></param>
		/// <param name="capacity"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Initialize(WorldState worldState, int capacity)
		{
			var size = capacity.GetPrime();
			_buckets = new MemArray<int>(worldState, size);
			_slots = new MemArray<Slot>(worldState, size);
			_freeList = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<Slot> GetSlotPtr(WorldState worldState)
		{
			E.ASSERT(IsCreated);
			return _slots.GetValuePtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			_buckets.Dispose(worldState);
			_slots.Dispose(worldState);
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr GetMemPtr()
		{
			E.ASSERT(IsCreated);
			return _buckets.innerArray.ptr.memPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReplaceWith(WorldState worldState, ref MemHashSet<T> other)
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
		/// <param name="worldState"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(WorldState worldState)
		{
			if (_lastIndex > 0)
			{
				// clear the elements so that the gc can reclaim the references.
				// clear only up to m_lastIndex for m_slots
				_slots.Clear(worldState, 0, _lastIndex);
				_buckets.Clear(worldState, 0, _buckets.Length);
				_lastIndex = 0;
				_count = 0;
				_freeList = -1;
				_hash = 0;
			}
		}

		/// <summary>
		/// Checks if this hashset contains the item
		/// </summary>
		/// <param name="worldState"></param>
		/// <param name="item">item to check for containment</param>
		/// <returns>true if item contained; false if not</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(WorldState worldState, in T item)
		{
			return Contains(item, _slots.GetValuePtr(worldState), _buckets.GetValuePtr(worldState));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly bool Contains(in T item, SafePtr<Slot> slotsPtr, SafePtr<int> bucketsPtr)
		{
			E.ASSERT(IsCreated);

			var hashCode = item.GetHashCode() & _hashCodeMask;
			// see note at "HashSet" level describing why "- 1" appears in for loop
			for (var i = bucketsPtr[hashCode % _buckets.Length] - 1; i >= 0; i = slotsPtr[i].next)
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
		public void Add(WorldState worldState, ref MemHashSet<T> other)
		{
			var slotsPtr = _slots.GetValuePtr(worldState);
			for (var i = 0; i < other._lastIndex; i++)
			{
				ref var slot = ref (slotsPtr + i).Value();
				if (slot.hashCode >= 0)
				{
					Add(worldState, slot.value);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveExcept(WorldState worldState, ref MemHashSet<T> other)
		{
			var slotsPtr = _slots.GetValuePtr(worldState);
			for (var i = 0; i < _lastIndex; i++)
			{
				ref var slot = ref slotsPtr[i];
				if (slot.hashCode >= 0)
				{
					var item = slot.value;
					if (!other.Contains(worldState, item))
					{
						Remove(worldState, item);
						if (_count == 0)
							return;
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(WorldState worldState, ref MemHashSet<T> other)
		{
			E.ASSERT(IsCreated);

			var slotsPtr = _slots.GetValuePtr(worldState);
			for (var i = 0; i < _lastIndex; i++)
			{
				ref var slot = ref (slotsPtr + i).Value();
				if (slot.hashCode >= 0)
				{
					var item = slot.value;
					if (other.Contains(worldState, item))
					{
						Remove(worldState, item);
						if (_count == 0)
							return;
					}
				}
			}
		}

		/// <summary>
		/// Remove item from this hashset
		/// </summary>
		/// <param name="worldState"></param>
		/// <param name="item">item to remove</param>
		/// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(WorldState worldState, in T item)
		{
			if (!_buckets.IsCreated)
				return false;

			var slotsSpan = _slots.GetSpan(worldState);
			var bucketsSpan = _buckets.GetSpan(worldState);

			var hashCode = item.GetHashCode() & _hashCodeMask;
			var bucket = hashCode % _buckets.Length;
			var last = -1;
			for (var i = bucketsSpan[bucket] - 1; i >= 0; last = i, i = slotsSpan[i].next)
			{
				if (slotsSpan[i].hashCode == hashCode && slotsSpan[i].value.Equals(item))
				{
					if (last < 0)
					{
						// first iteration; update buckets
						bucketsSpan[bucket] = slotsSpan[i].next + 1;
					}
					else
					{
						// subsequent iterations; update 'next' pointers
						slotsSpan[last].next = slotsSpan[i].next;
					}
					slotsSpan[i].hashCode = -1;
					slotsSpan[i].value = default(T);
					slotsSpan[i].next = _freeList;

					_count--;
					if (_count == 0)
					{
						_lastIndex = 0;
						_freeList = -1;
					}
					else
					{
						_freeList = i;
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
		private void IncreaseCapacity(WorldState worldState)
		{
			var newSize = _count.ExpandPrime();

			// Able to increase capacity; copy elements to larger array and rehash
			SetCapacity(worldState, newSize);
		}

		/// <summary>
		/// Set the underlying buckets array to size newSize and rehash.  Note that newSize
		/// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
		/// instead of this method.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetCapacity(WorldState worldState, int newSize)
		{
			var newSlots = new MemArray<Slot>(worldState, newSize);
			if (_slots.IsCreated)
			{
				MemArrayExt.CopyNoChecks<MemHashSet<T>.Slot>(worldState, _slots, 0, ref newSlots, 0, _lastIndex);
			}

			var newBuckets = new MemArray<int>(worldState, newSize);
			for (var i = 0; i < _lastIndex; i++)
			{
				var bucket = newSlots[worldState, i].hashCode % newSize;
				newSlots[worldState, i].next = newBuckets[worldState, bucket] - 1;
				newBuckets[worldState, bucket] = i + 1;
			}

			if (_slots.IsCreated)
				_slots.Dispose(worldState);
			if (_buckets.IsCreated)
				_buckets.Dispose(worldState);
			_slots = newSlots;
			_buckets = newBuckets;
		}

		/// <summary>
		/// Add item to this HashSet. Returns bool indicating whether item was added (won't be
		/// added if already present)
		/// </summary>
		/// <param name="worldState"></param>
		/// <param name="value"></param>
		/// <returns>true if added, false if already present</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Add(WorldState worldState, in T value)
		{
			if (!_buckets.IsCreated)
			{
				Initialize(worldState, 0);
			}

			var bucketsPtr = _buckets.GetValuePtr(worldState);
			var slotsPtr = _slots.GetValuePtr(worldState);

			var hashCode = value.GetHashCode() & _hashCodeMask;
			var bucket = hashCode % _buckets.Length;

			for (var i = bucketsPtr[bucket] - 1; i >= 0; i = slotsPtr[i].next)
			{
				if (slotsPtr[i].hashCode == hashCode && slotsPtr[i].value.Equals(value))
				{
					return false;
				}
			}

			int index;
			if (_freeList >= 0)
			{
				index = _freeList;
				_freeList = slotsPtr[index].next;
			}
			else
			{
				if (_lastIndex == _slots.Length)
				{
					IncreaseCapacity(worldState);
					// this will change during resize
					bucket = hashCode % _buckets.Length;

					bucketsPtr = _buckets.GetValuePtr(worldState);
					slotsPtr = _slots.GetValuePtr(worldState);
				}
				index = _lastIndex;
				_lastIndex++;
			}

			slotsPtr[index].hashCode = hashCode;
			slotsPtr[index].value = value;
			slotsPtr[index].next = bucketsPtr[bucket] - 1;
			bucketsPtr[bucket] = index + 1;

			_count++;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int GetHash()
		{
			return _hash;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(WorldState worldState, in MemHashSet<T> other)
		{
			MemArrayExt.CopyExact(worldState, in other._buckets, ref _buckets);

			_slots.CopyFrom(worldState, other._slots);
			var thisBuckets = _buckets;
			var thisSlots = _slots;
			this = other;

			_buckets = thisBuckets;
			_slots = thisSlots;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemHashSetEnumerator<T> GetEnumerator(WorldState worldState)
		{
			if (Count == 0)
				return default;
			return new MemHashSetEnumerator<T>(GetSlotPtr(worldState), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemHashSetEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}

		private class HashSetProxy
		{
			private MemHashSet<T> _hashSet;

			public HashSetProxy(MemHashSet<T> hashSet)
			{
				_hashSet = hashSet;
			}

			public MemArray<int> Buckets => _hashSet._buckets;
			public MemArray<MemHashSet<T>.Slot> Slots => _hashSet._slots;
			public int Count => _hashSet._count;
			public int FreeList => _hashSet._freeList;
			public int LastIndex => _hashSet._lastIndex;

			public T[] Items
			{
				get
				{
#if DEBUG
					var arr = new T[_hashSet.Count];
					var i = 0;
					var worldState = _hashSet._buckets.GetWorldState_DEBUG();
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
