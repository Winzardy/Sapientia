using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public enum InsertionBehavior
	{
		None = 0,
		OverwriteExisting,
		ThrowOnExisting,
	}

	[DebuggerTypeProxy(typeof(MemDictionary<,>.EquatableDictionaryProxy))]
	public struct MemDictionary<TKey, TValue> : IMemDictionaryEnumerable<TKey, TValue>
		where TKey : unmanaged, IEquatable<TKey>
		where TValue : unmanaged
	{
		public struct Entry
		{
			public int hashCode; // Lower 31 bits of hash code, -1 if unused
			public int next; // Index of next entry, -1 if last
			public TKey key; // Key of entry
			public TValue value; // Value of entry
		}

		private const int _hashCodeMask = 0x7FFFFFFF;

		internal MemArray<int> buckets;
		internal MemArray<Entry> entries;
		internal int count;
		internal int freeList;
		internal int freeCount;

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => buckets.IsCreated;
		}

		public readonly int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => count - freeCount;
		}

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => buckets.Length;
		}

		public readonly int LastIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemDictionary(WorldState worldState, in MemDictionary<TKey, TValue> other)
		{
			E.ASSERT(other.IsCreated);

			this = other;
			buckets = new MemArray<int>(worldState, other.buckets);
			entries = new MemArray<Entry>(worldState, other.entries);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemDictionary(WorldState worldState, int capacity = 8)
		{
			this = default;
			Initialize(worldState, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Initialize(WorldState worldState, int capacity)
		{
			var prime = capacity.GetPrime();
			freeList = -1;
			buckets = new MemArray<int>(worldState, prime, -1);
			entries = new MemArray<Entry>(worldState, prime);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			buckets.Dispose(worldState);
			entries.Dispose(worldState);
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<Entry> GetEntryPtr(WorldState worldState)
		{
			return entries.GetValuePtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly MemPtr GetMemPtr()
		{
			return buckets.innerArray.ptr.memPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReplaceWith(WorldState worldState, in MemDictionary<TKey, TValue> other)
		{
			if (GetMemPtr() == other.GetMemPtr()) return;

			Dispose(worldState);
			this = other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(WorldState worldState, in MemDictionary<TKey, TValue> other)
		{
			if (GetMemPtr() == other.GetMemPtr())
				return;
			if (!GetMemPtr().IsValid() && !other.GetMemPtr().IsValid())
				return;
			if (GetMemPtr().IsValid() && !other.GetMemPtr().IsValid())
			{
				Dispose(worldState);
				return;
			}

			if (!GetMemPtr().IsValid())
				this = new MemDictionary<TKey, TValue>(worldState, other.Count);

			MemArrayExt.CopyExact(worldState, other.buckets, ref buckets);
			MemArrayExt.CopyExact(worldState, other.entries, ref entries);
			count = other.count;
			freeCount = other.freeCount;
			freeList = other.freeList;
		}

		/// <summary><para>Gets or sets the value associated with the specified key.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key whose value is to be gotten or set.</param>
		public ref TValue this[WorldState worldState, in TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				var entry = FindEntry(worldState, key);
				if (entry >= 0)
				{
					return ref entries[worldState, entry].value;
				}

				throw new KeyNotFoundException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TValue GetValue(WorldState worldState, TKey key)
		{
			var entry = FindEntry(worldState, key);
			if (entry >= 0)
			{
				return ref entries[worldState, entry].value;
			}

			return ref UnsafeExt.DefaultRef<TValue>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TValue GetValue(WorldState worldState, TKey key, out bool success)
		{
			var entry = FindEntry(worldState, key);
			if (entry >= 0)
			{
				success = true;
				return ref entries[worldState, entry].value;
			}

			success = false;
			return ref UnsafeExt.DefaultRef<TValue>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TValue GetOrCreateValue(WorldState worldState, TKey key, TValue defaultValue = default)
		{
			var entry = FindEntry(worldState, key);
			if (entry >= 0)
			{
				return ref entries[worldState, entry].value;
			}

			Add(worldState, key, defaultValue);
			return ref GetValue(worldState, key);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TValue GetOrCreateValue(WorldState worldState, TKey key, out bool isCreated, TValue defaultValue = default)
		{
			var entry = FindEntry(worldState, key);
			if (entry >= 0)
			{
				isCreated = false;
				return ref entries[worldState, entry].value;
			}

			Add(worldState, key, defaultValue);
			isCreated = true;
			return ref GetValue(worldState, key);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(WorldState worldState, TKey key, out TValue value)
		{
			var entry = FindEntry(worldState, key);
			if (entry >= 0)
			{
				value = entries[worldState, entry].value;
				return true;
			}

			value = UnsafeExt.DefaultRef<TValue>();
			return false;
		}

		/// <summary><para>Adds an element with the specified key and value to the dictionary.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key of the element to add to the dictionary.</param>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(WorldState worldState, TKey key, TValue value)
		{
			TryInsert(worldState, key, value, InsertionBehavior.ThrowOnExisting);
		}

		/// <summary><para>Removes all elements from the dictionary.</para></summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(WorldState worldState)
		{
			var clearCount = count;
			if (clearCount > 0)
			{
				buckets.Fill(worldState, -1);
				count = 0;
				freeList = -1;
				freeCount = 0;
				entries.Clear(worldState, 0, clearCount);
			}
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific key.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key to locate in the dictionary.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsKey(WorldState worldState, TKey key)
		{
			return FindEntry(worldState, key) >= 0;
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific value.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="value">The value to locate in the dictionary.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsValue(WorldState worldState, TValue value)
		{
			var rawEntries = entries.GetValuePtr(worldState);
			for (var i = 0; i < count; i++)
			{
				if (rawEntries[i].hashCode >= 0 && value.Equals(rawEntries[i].value))
					return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindEntry(WorldState worldState, in TKey key)
		{
			if (buckets.IsCreated)
			{
				var hashCode = key.GetHashCode() & _hashCodeMask;
				var rawBuckets = buckets.GetValuePtr(worldState);
				var rawEntries = entries.GetValuePtr(worldState);

				for (var i = rawBuckets[hashCode % buckets.Length]; i >= 0; i = rawEntries[i].next)
				{
					if (rawEntries[i].hashCode == hashCode && rawEntries[i].key.Equals(key))
						return i;
				}
			}

			return -1;
		}

		private bool TryInsert(WorldState worldState, TKey key, TValue value, InsertionBehavior behavior)
		{
			if (!buckets.IsCreated)
			{
				Initialize(worldState, 0);
			}

			var hashCode = key.GetHashCode() & _hashCodeMask;
			var targetBucket = hashCode % buckets.Length;

			var rawBuckets = buckets.GetValuePtr(worldState);
			var rawEntries = entries.GetValuePtr(worldState);

			for (var i = rawBuckets[targetBucket]; i >= 0; i = rawEntries[i].next)
			{
				if (rawEntries[i].hashCode == hashCode && rawEntries[i].key.Equals(key))
				{
					switch (behavior)
					{
						case InsertionBehavior.OverwriteExisting:
							rawEntries[i].value = value;
							return true;

						case InsertionBehavior.ThrowOnExisting:
							E.ADDING_DUPLICATE();
							break;
					}

					return false;
				}
			}

			int index;
			if (freeCount > 0)
			{
				index = freeList;
				freeList = rawEntries[index].next;
				freeCount--;
			}
			else
			{
				if (count == entries.Length)
				{
					IncreaseCapacity(worldState);
					targetBucket = hashCode % buckets.Length;

					rawBuckets = buckets.GetValuePtr(worldState);
					rawEntries = entries.GetValuePtr(worldState);
				}

				index = count;
				count++;
			}

			rawEntries[index].hashCode = hashCode;
			rawEntries[index].next = buckets[worldState, targetBucket];
			rawEntries[index].key = key;
			rawEntries[index].value = value;
			rawBuckets[targetBucket] = index;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncreaseCapacity(WorldState worldState)
		{
			IncreaseCapacity(worldState, count.ExpandPrime());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncreaseCapacity(WorldState worldState, int newSize)
		{
			var bucketsArray = new MemArray<int>(worldState, newSize, -1);
			var entryArray = new MemArray<Entry>(worldState, newSize);

			MemArrayExt.CopyNoChecks(worldState, entries, 0, ref entryArray, 0, count);

			var rawBuckets = bucketsArray.GetValuePtr(worldState);
			var rawEntries = entryArray.GetValuePtr(worldState);

			for (var i = 0; i < count; i++)
			{
				if (rawEntries[i].hashCode >= 0)
				{
					var bucket = rawEntries[i].hashCode % newSize;
					rawEntries[i].next = rawBuckets[bucket];
					rawBuckets[bucket] = i;
				}
			}

			buckets.Dispose(worldState);
			entries.Dispose(worldState);

			buckets = bucketsArray;
			entries = entryArray;
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(WorldState worldState, TKey key)
		{
			return Remove(worldState, key, out _);
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(WorldState worldState, TKey key, out TValue value)
		{
			value = default;

			if (!buckets.IsCreated)
				return false;

			var hashCode = key.GetHashCode() & _hashCodeMask;
			var bucket = hashCode % buckets.Length;
			var last = -1;

			var rawBuckets = buckets.GetValuePtr(worldState);
			var rawEntries = entries.GetValuePtr(worldState);

			for (var i = rawBuckets[bucket]; i >= 0; last = i, i = rawEntries[i].next)
			{
				if (rawEntries[i].hashCode == hashCode && rawEntries[i].key.Equals(key))
				{
					if (last < 0)
					{
						rawBuckets[bucket] = rawEntries[i].next;
					}
					else
					{
						entries[worldState, last].next = rawEntries[i].next;
					}

					value = rawEntries[i].value;

					rawEntries[i].hashCode = -1;
					rawEntries[i].next = freeList;
					rawEntries[i].key = default(TKey);
					rawEntries[i].value = default(TValue);

					freeList = i;
					freeCount++;

					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryAdd(WorldState worldState, TKey key, TValue value)
		{
			return TryInsert(worldState, key, value, InsertionBehavior.None);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int EnsureCapacity(WorldState worldState, int capacity)
		{
			E.ASSERT(IsCreated);

			var num = entries.Length;
			if (num >= capacity)
			{
				return num;
			}

			if (buckets.IsCreated == false)
			{
				Initialize(worldState, capacity);
				return Capacity;
			}

			var prime = capacity.GetPrime();
			IncreaseCapacity(worldState, prime);
			return prime;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemDictionaryEnumerator<TKey, TValue> GetEnumerator(WorldState worldState)
		{
			return new MemDictionaryEnumerator<TKey, TValue>(GetEntryPtr(worldState), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemDictionaryEnumerable<TKey, TValue> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}

		private class EquatableDictionaryProxy
		{
			private MemDictionary<TKey, TValue> _dictionary;

			public EquatableDictionaryProxy(MemDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
			}

			public MemArray<int> Buckets => _dictionary.buckets;
			public MemArray<Entry> Entries => _dictionary.entries;
			public int Count => _dictionary.count;
			public int FreeList => _dictionary.freeList;
			public int FreeCount => _dictionary.freeCount;

			public KeyValuePair<TKey, TValue>[] Items
			{
				get
				{
#if DEBUG
					var arr = new KeyValuePair<TKey, TValue>[_dictionary.Count];
					var i = 0;
					var worldState = _dictionary.buckets.GetWorldState_DEBUG();
					var e = _dictionary.GetEnumerator(worldState);
					while (e.MoveNext())
					{
						arr[i++] = new KeyValuePair<TKey, TValue>(e.Current.key, e.Current.value);
					}

					return arr;
#else
					return Array.Empty<KeyValuePair<TKey, TValue>>();
#endif
				}
			}
		}
	}
}
