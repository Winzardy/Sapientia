using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
		public Allocator* GetAllocatorPtr()
		{
			return buckets.GetAllocatorPtr();
		}

		[INLINE(256)]
		public Dictionary(Allocator* allocator, int capacity)
		{
			this = default;
			Initialize(allocator, capacity);
		}

		[INLINE(256)]
		private void Initialize(Allocator* allocator, int capacity)
		{
			var prime = capacity.GetPrime();
			freeList = -1;
			buckets = new MemArray<int>(allocator, prime, -1);
			entries = new MemArray<Entry>(allocator, prime);
		}

		[INLINE(256)]
		public void Dispose(Allocator* allocator)
		{
			buckets.Dispose(allocator);
			entries.Dispose(allocator);
			this = default;
		}

		[INLINE(256)]
		public Entry* GetEntryPtr()
		{
			return entries.GetValuePtr();
		}

		[INLINE(256)]
		public Entry* GetEntryPtr(Allocator* allocator)
		{
			return entries.GetValuePtr(allocator);
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			return buckets.innerArray.ptr.memPtr;
		}

		[INLINE(256)]
		public void ReplaceWith(Allocator* allocator, in Dictionary<TKey, TValue> other)
		{
			if (GetMemPtr() == other.GetMemPtr()) return;

			Dispose(allocator);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(Allocator* allocator, in Dictionary<TKey, TValue> other)
		{
			if (GetMemPtr() == other.GetMemPtr())
				return;
			if (!GetMemPtr().IsValid() && !other.GetMemPtr().IsValid())
				return;
			if (GetMemPtr().IsValid() && !other.GetMemPtr().IsValid())
			{
				Dispose(allocator);
				return;
			}

			if (GetMemPtr().IsValid() == false)
				this = new Dictionary<TKey, TValue>(allocator, other.Count);

			MemArrayExt.CopyExact(allocator, other.buckets, ref buckets);
			MemArrayExt.CopyExact(allocator, other.entries, ref entries);
			count = other.count;
			freeCount = other.freeCount;
			freeList = other.freeList;
		}

		/// <summary><para>Gets or sets the value associated with the specified key.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key whose value is to be gotten or set.</param>
		public ref TValue this[Allocator* allocator, in TKey key]
		{
			[INLINE(256)]
			get
			{
				var entry = FindEntry(allocator, key);
				if (entry >= 0)
				{
					return ref entries[allocator, entry].value;
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
				var allocator = GetAllocatorPtr();
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
			var allocator = GetAllocatorPtr();
			return ref GetValue(allocator, key);
		}

		[INLINE(256)]
		public ref TValue GetValue(Allocator* allocator, TKey key)
		{
			var entry = FindEntry(allocator, key);
			if (entry >= 0)
			{
				return ref entries[allocator, entry].value;
			}

			return ref TDefaultValue<TValue>.value;
		}

		[INLINE(256)]
		public ref TValue GetValue(Allocator* allocator, TKey key, out bool success)
		{
			var entry = FindEntry(allocator, key);
			if (entry >= 0)
			{
				success = true;
				return ref entries[allocator, entry].value;
			}

			success = false;
			return ref TDefaultValue<TValue>.value;
		}

		[INLINE(256)]
		public bool TryGetValue(Allocator* allocator, TKey key, out TValue value)
		{
			var entry = FindEntry(allocator, key);
			if (entry >= 0)
			{
				value = entries[allocator, entry].value;
				return true;
			}

			value = TDefaultValue<TValue>.value;
			return false;
		}

		[INLINE(256)]
		public bool TryGetValuePtr(Allocator* allocator, TKey key, out TValue* value)
		{
			var entry = FindEntry(allocator, key);
			if (entry >= 0)
			{
				value = (TValue*)entries[allocator, entry].value.AsPointer();
				return true;
			}

			value = null;
			return false;
		}

		/// <summary><para>Adds an element with the specified key and value to the dictionary.</para></summary>
		/// <param name="key">The key of the element to add to the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public void Add(TKey key, TValue value)
		{
			TryInsert(GetAllocatorPtr(), key, value, InsertionBehavior.ThrowOnExisting);
		}

		/// <summary><para>Adds an element with the specified key and value to the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to add to the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public void Add(Allocator* allocator, TKey key, TValue value)
		{
			TryInsert(allocator, key, value, InsertionBehavior.ThrowOnExisting);
		}

		/// <summary><para>Removes all elements from the dictionary.</para></summary>
		[INLINE(256)]
		public void Clear(Allocator* allocator)
		{
			var clearCount = count;
			if (clearCount > 0)
			{
				buckets.Clear(allocator);
				count = 0;
				freeList = -1;
				freeCount = 0;
				entries.Clear(allocator, 0, clearCount);
			}
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific key.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key to locate in the dictionary.</param>
		[INLINE(256)]
		public bool ContainsKey(Allocator* allocator, TKey key)
		{
			return FindEntry(allocator, key) >= 0;
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific value.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="value">The value to locate in the dictionary.</param>
		[INLINE(256)]
		public bool ContainsValue(Allocator* allocator, TValue value)
		{
			var rawEntries = entries.GetValuePtr(allocator);
			for (var i = 0; i < count; i++)
			{
				if (rawEntries[i].hashCode >= 0 && value.Equals(rawEntries[i].value))
					return true;
			}

			return false;
		}

		[INLINE(256)]
		private int FindEntry(Allocator* allocator, in TKey key)
		{
			if (buckets.IsCreated)
			{
				var hashCode = key.GetHashCode() & _hashCodeMask;
				var rawBuckets = buckets.GetValuePtr(allocator);
				var rawEntries = entries.GetValuePtr(allocator);

				for (var i = rawBuckets[hashCode % buckets.Length]; i >= 0; i = rawEntries[i].next)
				{
					if (rawEntries[i].hashCode == hashCode && rawEntries[i].key.Equals(key))
						return i;
				}
			}

			return -1;
		}

		private bool TryInsert(Allocator* allocator, TKey key, TValue value, InsertionBehavior behavior)
		{
			if (!buckets.IsCreated)
			{
				Initialize(allocator, 0);
			}

			var hashCode = key.GetHashCode() & _hashCodeMask;
			var targetBucket = hashCode % buckets.Length;

			var rawBuckets = buckets.GetValuePtr(allocator);
			var rawEntries = entries.GetValuePtr(allocator);

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
					Resize(allocator);
					targetBucket = hashCode % buckets.Length;

					rawBuckets = buckets.GetValuePtr(allocator);
					rawEntries = entries.GetValuePtr(allocator);
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
		private void Resize(Allocator* allocator)
		{
			Resize(allocator, count.ExpandPrime());
		}

		[INLINE(256)]
		private void Resize(Allocator* allocator, int newSize)
		{
			var bucketsArray = new MemArray<int>(allocator, newSize, -1);
			var entryArray = new MemArray<Entry>(allocator, newSize);

			MemArrayExt.CopyNoChecks(allocator, entries, 0, ref entryArray, 0, count);

			var rawBuckets = bucketsArray.GetValuePtr(allocator);
			var rawEntries = entryArray.GetValuePtr(allocator);

			for (var i = 0; i < count; i++)
			{
				if (rawEntries[i].hashCode >= 0)
				{
					var bucket = rawEntries[i].hashCode % newSize;
					rawEntries[i].next = rawBuckets[bucket];
					rawBuckets[bucket] = i;
				}
			}

			buckets.Dispose(allocator);
			entries.Dispose(allocator);

			buckets = bucketsArray;
			entries = entryArray;
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		[INLINE(256)]
		public bool Remove(TKey key)
		{
			return Remove(GetAllocatorPtr(), key);
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public bool Remove(Allocator* allocator, TKey key)
		{
			return Remove(allocator, key, out _);
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public bool Remove(Allocator* allocator, TKey key, out TValue value)
		{
			value = default;

			if (!buckets.IsCreated)
				return false;

			var hashCode = key.GetHashCode() & _hashCodeMask;
			var bucket = hashCode % buckets.Length;
			var last = -1;

			var rawBuckets = buckets.GetValuePtr(allocator);
			var rawEntries = entries.GetValuePtr(allocator);

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
		public bool TryAdd(Allocator* allocator, TKey key, TValue value)
		{
			return TryInsert(allocator, key, value, InsertionBehavior.None);
		}

		[INLINE(256)]
		public int EnsureCapacity(Allocator* allocator, int capacity)
		{
			Debug.Assert(IsCreated);

			var num = entries.Length;
			if (num >= capacity)
			{
				return num;
			}

			if (buckets.IsCreated == false)
			{
				Initialize(allocator, capacity);
				return Capacity;
			}

			var prime = capacity.GetPrime();
			Resize(allocator, prime);
			return prime;
		}

		[INLINE(256)]
		public DictionaryEnumerator<TKey, TValue> GetEnumerator(Allocator* allocator)
		{
			return new DictionaryEnumerator<TKey, TValue>(GetEntryPtr(allocator), LastIndex);
		}

		[INLINE(256)]
		public new DictionaryEnumerator<TKey, TValue> GetEnumerator()
		{
			return new DictionaryEnumerator<TKey, TValue>(GetEntryPtr(), LastIndex);
		}

		[INLINE(256)]
		public DictionaryPtrEnumerator<TKey, TValue> GetPtrEnumerator(Allocator* allocator)
		{
			return new DictionaryPtrEnumerator<TKey, TValue>(GetEntryPtr(allocator), LastIndex);
		}

		[INLINE(256)]
		public DictionaryPtrEnumerator<TKey, TValue> GetPtrEnumerator()
		{
			return new DictionaryPtrEnumerator<TKey, TValue>(GetEntryPtr(), LastIndex);
		}

		[INLINE(256)]
		public Enumerable<Entry, DictionaryEnumerator<TKey, TValue>> GetEnumerable(Allocator* allocator)
		{
			return new(new(GetEntryPtr(allocator), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<Entry, DictionaryEnumerator<TKey, TValue>> GetEnumerable()
		{
			return new(new(GetEntryPtr(), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, DictionaryPtrEnumerator<TKey, TValue>> GetPtrEnumerable(Allocator* allocator)
		{
			return new(new(GetEntryPtr(allocator), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, DictionaryPtrEnumerator<TKey, TValue>> GetPtrEnumerable()
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
