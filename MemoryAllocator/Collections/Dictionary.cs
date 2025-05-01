using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public enum InsertionBehavior
	{
		None = 0,
		OverwriteExisting,
		ThrowOnExisting,
	}

	[DebuggerTypeProxy(typeof(EquatableDictionaryProxy<,>))]
	public unsafe struct Dictionary<TKey, TValue> : IDictionaryEnumerable<TKey, TValue>
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
			[INLINE(256)] get => buckets.IsCreated;
		}

		public readonly int Count
		{
			[INLINE(256)] get => count - freeCount;
		}

		public readonly int Capacity
		{
			[INLINE(256)] get => buckets.Length;
		}

		public readonly int LastIndex
		{
			[INLINE(256)] get => count;
		}

		[INLINE(256)]
		public World GetAllocator()
		{
			return buckets.GetAllocator();
		}

		[INLINE(256)]
		public Dictionary(World world, int capacity)
		{
			this = default;
			Initialize(world, capacity);
		}

		[INLINE(256)]
		private void Initialize(World world, int capacity)
		{
			var prime = capacity.GetPrime();
			freeList = -1;
			buckets = new MemArray<int>(world, prime, -1);
			entries = new MemArray<Entry>(world, prime);
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			buckets.Dispose(world);
			entries.Dispose(world);
			this = default;
		}

		[INLINE(256)]
		public SafePtr<Entry> GetEntryPtr()
		{
			return entries.GetValuePtr();
		}

		[INLINE(256)]
		public SafePtr<Entry> GetEntryPtr(World world)
		{
			return entries.GetValuePtr(world);
		}

		[INLINE(256)]
		public readonly WPtr GetMemPtr()
		{
			return buckets.innerArray.ptr.wPtr;
		}

		[INLINE(256)]
		public void ReplaceWith(World world, in Dictionary<TKey, TValue> other)
		{
			if (GetMemPtr() == other.GetMemPtr()) return;

			Dispose(world);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(World world, in Dictionary<TKey, TValue> other)
		{
			if (GetMemPtr() == other.GetMemPtr())
				return;
			if (!GetMemPtr().IsCreated() && !other.GetMemPtr().IsCreated())
				return;
			if (GetMemPtr().IsCreated() && !other.GetMemPtr().IsCreated())
			{
				Dispose(world);
				return;
			}

			if (GetMemPtr().IsCreated() == false)
				this = new Dictionary<TKey, TValue>(world, other.Count);

			MemArrayExt.CopyExact(world, other.buckets, ref buckets);
			MemArrayExt.CopyExact(world, other.entries, ref entries);
			count = other.count;
			freeCount = other.freeCount;
			freeList = other.freeList;
		}

		/// <summary><para>Gets or sets the value associated with the specified key.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key whose value is to be gotten or set.</param>
		public ref TValue this[World world, in TKey key]
		{
			[INLINE(256)]
			get
			{
				var entry = FindEntry(world, key);
				if (entry >= 0)
				{
					return ref entries[world, entry].value;
				}

				throw new KeyNotFoundException();
			}
		}

		/// <summary><para>Gets or sets the value associated with the specified key.</para></summary>
		/// <param name="key">The key whose value is to be gotten or set.</param>
		public ref TValue this[in TKey key]
		{
			[INLINE(256)]
			get
			{
				var allocator = GetAllocator();
				var entry = FindEntry(allocator, key);
				if (entry >= 0)
				{
					return ref entries[allocator, entry].value;
				}

				throw new KeyNotFoundException();
			}
		}

		[INLINE(256)]
		public ref TValue GetValue(TKey key)
		{
			var allocator = GetAllocator();
			return ref GetValue(allocator, key);
		}

		[INLINE(256)]
		public ref TValue GetValue(World world, TKey key)
		{
			var entry = FindEntry(world, key);
			if (entry >= 0)
			{
				return ref entries[world, entry].value;
			}

			return ref TDefaultValue<TValue>.value;
		}

		[INLINE(256)]
		public ref TValue GetValue(World world, TKey key, out bool success)
		{
			var entry = FindEntry(world, key);
			if (entry >= 0)
			{
				success = true;
				return ref entries[world, entry].value;
			}

			success = false;
			return ref TDefaultValue<TValue>.value;
		}

		[INLINE(256)]
		public bool TryGetValue(World world, TKey key, out TValue value)
		{
			var entry = FindEntry(world, key);
			if (entry >= 0)
			{
				value = entries[world, entry].value;
				return true;
			}

			value = TDefaultValue<TValue>.value;
			return false;
		}

		/// <summary><para>Adds an element with the specified key and value to the dictionary.</para></summary>
		/// <param name="key">The key of the element to add to the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public void Add(TKey key, TValue value)
		{
			TryInsert(GetAllocator(), key, value, InsertionBehavior.ThrowOnExisting);
		}

		/// <summary><para>Adds an element with the specified key and value to the dictionary.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key of the element to add to the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public void Add(World world, TKey key, TValue value)
		{
			TryInsert(world, key, value, InsertionBehavior.ThrowOnExisting);
		}

		/// <summary><para>Removes all elements from the dictionary.</para></summary>
		[INLINE(256)]
		public void Clear(World world)
		{
			var clearCount = count;
			if (clearCount > 0)
			{
				buckets.Clear(world);
				count = 0;
				freeList = -1;
				freeCount = 0;
				entries.Clear(world, 0, clearCount);
			}
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific key.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key to locate in the dictionary.</param>
		[INLINE(256)]
		public bool ContainsKey(World world, TKey key)
		{
			return FindEntry(world, key) >= 0;
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific value.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="value">The value to locate in the dictionary.</param>
		[INLINE(256)]
		public bool ContainsValue(World world, TValue value)
		{
			var rawEntries = entries.GetValuePtr(world);
			for (var i = 0; i < count; i++)
			{
				if (rawEntries[i].hashCode >= 0 && value.Equals(rawEntries[i].value))
					return true;
			}

			return false;
		}

		[INLINE(256)]
		private int FindEntry(World world, in TKey key)
		{
			if (buckets.IsCreated)
			{
				var hashCode = key.GetHashCode() & _hashCodeMask;
				var rawBuckets = buckets.GetValuePtr(world);
				var rawEntries = entries.GetValuePtr(world);

				for (var i = rawBuckets[hashCode % buckets.Length]; i >= 0; i = rawEntries[i].next)
				{
					if (rawEntries[i].hashCode == hashCode && rawEntries[i].key.Equals(key))
						return i;
				}
			}

			return -1;
		}

		private bool TryInsert(World world, TKey key, TValue value, InsertionBehavior behavior)
		{
			if (!buckets.IsCreated)
			{
				Initialize(world, 0);
			}

			var hashCode = key.GetHashCode() & _hashCodeMask;
			var targetBucket = hashCode % buckets.Length;

			var rawBuckets = buckets.GetValuePtr(world);
			var rawEntries = entries.GetValuePtr(world);

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
					IncreaseCapacity(world);
					targetBucket = hashCode % buckets.Length;

					rawBuckets = buckets.GetValuePtr(world);
					rawEntries = entries.GetValuePtr(world);
				}

				index = count;
				count++;
			}

			rawEntries[index].hashCode = hashCode;
			rawEntries[index].next = buckets[targetBucket];
			rawEntries[index].key = key;
			rawEntries[index].value = value;
			rawBuckets[targetBucket] = index;

			return true;
		}

		[INLINE(256)]
		private void IncreaseCapacity(World world)
		{
			IncreaseCapacity(world, count.ExpandPrime());
		}

		[INLINE(256)]
		private void IncreaseCapacity(World world, int newSize)
		{
			var bucketsArray = new MemArray<int>(world, newSize, -1);
			var entryArray = new MemArray<Entry>(world, newSize);

			MemArrayExt.CopyNoChecks(world, entries, 0, ref entryArray, 0, count);

			var rawBuckets = bucketsArray.GetValuePtr(world);
			var rawEntries = entryArray.GetValuePtr(world);

			for (var i = 0; i < count; i++)
			{
				if (rawEntries[i].hashCode >= 0)
				{
					var bucket = rawEntries[i].hashCode % newSize;
					rawEntries[i].next = rawBuckets[bucket];
					rawBuckets[bucket] = i;
				}
			}

			buckets.Dispose(world);
			entries.Dispose(world);

			buckets = bucketsArray;
			entries = entryArray;
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		[INLINE(256)]
		public bool Remove(TKey key)
		{
			return Remove(GetAllocator(), key);
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public bool Remove(World world, TKey key)
		{
			return Remove(world, key, out _);
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public bool Remove(World world, TKey key, out TValue value)
		{
			value = default;

			if (!buckets.IsCreated)
				return false;

			var hashCode = key.GetHashCode() & _hashCodeMask;
			var bucket = hashCode % buckets.Length;
			var last = -1;

			var rawBuckets = buckets.GetValuePtr(world);
			var rawEntries = entries.GetValuePtr(world);

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
						entries[last].next = rawEntries[i].next;
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

		[INLINE(256)]
		public bool TryAdd(World world, TKey key, TValue value)
		{
			return TryInsert(world, key, value, InsertionBehavior.None);
		}

		[INLINE(256)]
		public int EnsureCapacity(World world, int capacity)
		{
			E.ASSERT(IsCreated);

			var num = entries.Length;
			if (num >= capacity)
			{
				return num;
			}

			if (buckets.IsCreated == false)
			{
				Initialize(world, capacity);
				return Capacity;
			}

			var prime = capacity.GetPrime();
			IncreaseCapacity(world, prime);
			return prime;
		}

		[INLINE(256)]
		public DictionaryEnumerator<TKey, TValue> GetEnumerator(World world)
		{
			return new DictionaryEnumerator<TKey, TValue>(GetEntryPtr(world), LastIndex);
		}

		[INLINE(256)]
		public new DictionaryEnumerator<TKey, TValue> GetEnumerator()
		{
			return new DictionaryEnumerator<TKey, TValue>(GetEntryPtr(), LastIndex);
		}

		[INLINE(256)]
		public DictionaryPtrEnumerator<TKey, TValue> GetPtrEnumerator(World world)
		{
			return new DictionaryPtrEnumerator<TKey, TValue>(GetEntryPtr(world), LastIndex);
		}

		[INLINE(256)]
		public DictionaryPtrEnumerator<TKey, TValue> GetPtrEnumerator()
		{
			return new DictionaryPtrEnumerator<TKey, TValue>(GetEntryPtr(), LastIndex);
		}

		[INLINE(256)]
		public Enumerable<Entry, DictionaryEnumerator<TKey, TValue>> GetEnumerable(World world)
		{
			return new(new(GetEntryPtr(world), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<Entry, DictionaryEnumerator<TKey, TValue>> GetEnumerable()
		{
			return new(new(GetEntryPtr(), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<TValue>, DictionaryPtrEnumerator<TKey, TValue>> GetPtrEnumerable(World world)
		{
			return new(new(GetEntryPtr(world), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<TValue>, DictionaryPtrEnumerator<TKey, TValue>> GetPtrEnumerable()
		{
			return new(new(GetEntryPtr(), LastIndex));
		}

		[INLINE(256)]
		IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[INLINE(256)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
