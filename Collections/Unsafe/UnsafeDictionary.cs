using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.Collections
{
	public enum InsertionBehavior
	{
		None = 0,
		OverwriteExisting,
		ThrowOnExisting,
	}

	[DebuggerTypeProxy(typeof(UnsafeDictionary<,>.EquatableDictionaryProxy))]
	public struct UnsafeDictionary<TKey, TValue> : IEnumerable<UnsafeDictionary<TKey, TValue>.Entry>, IEnumerable<SafePtr<UnsafeDictionary<TKey, TValue>.Entry>>
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

		internal UnsafeArray<int> buckets;
		internal UnsafeArray<Entry> entries;
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

		public UnsafeDictionary(int capacity) : this(default, capacity)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UnsafeDictionary(Id<MemoryManager> memoryId, int capacity)
		{
			this = default;
			Initialize(memoryId, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Initialize(Id<MemoryManager> memoryId, int capacity)
		{
			var prime = capacity.GetPrime();
			freeList = -1;
			buckets = new UnsafeArray<int>(memoryId, prime);
			buckets.Fill(-1);
			entries = new UnsafeArray<Entry>(memoryId, prime);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			buckets.Dispose();
			entries.Dispose();
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<Entry> GetEntryPtr()
		{
			return entries.ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReplaceWith(in UnsafeDictionary<TKey, TValue> other)
		{
			if (GetEntryPtr() == other.GetEntryPtr())
				return;

			Dispose();
			this = other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(in UnsafeDictionary<TKey, TValue> other)
		{
			if (GetEntryPtr() == other.GetEntryPtr())
				return;
			if (!GetEntryPtr().IsValid && !other.GetEntryPtr().IsValid)
				return;
			if (GetEntryPtr().IsValid && !other.GetEntryPtr().IsValid)
			{
				Dispose();
				return;
			}

			if (!GetEntryPtr().IsValid)
				this = new UnsafeDictionary<TKey, TValue>(other.Count);

			buckets.CopyFrom(other.buckets);
			entries.CopyFrom(other.entries);

			count = other.count;
			freeCount = other.freeCount;
			freeList = other.freeList;
		}

		/// <summary><para>Gets or sets the value associated with the specified key.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key whose value is to be gotten or set.</param>
		public ref TValue this[in TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				var entry = FindEntry(key);
				if (entry >= 0)
				{
					return ref entries[entry].value;
				}

				throw new KeyNotFoundException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TValue GetValue( TKey key)
		{
			var entry = FindEntry(key);
			if (entry >= 0)
			{
				return ref entries[entry].value;
			}

			return ref UnsafeExt.DefaultRef<TValue>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TValue GetValue(TKey key, out bool success)
		{
			var entry = FindEntry(key);
			if (entry >= 0)
			{
				success = true;
				return ref entries[entry].value;
			}

			success = false;
			return ref UnsafeExt.DefaultRef<TValue>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(TKey key, out TValue value)
		{
			var entry = FindEntry(key);
			if (entry >= 0)
			{
				value = entries[entry].value;
				return true;
			}

			value = UnsafeExt.DefaultRefReadonly<TValue>();
			return false;
		}

		/// <summary><para>Adds an element with the specified key and value to the dictionary.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key of the element to add to the dictionary.</param>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(TKey key, TValue value)
		{
			TryInsert(key, value, InsertionBehavior.ThrowOnExisting);
		}

		/// <summary><para>Removes all elements from the dictionary.</para></summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			var clearCount = count;
			if (clearCount > 0)
			{
				buckets.Fill(-1);
				count = 0;
				freeList = -1;
				freeCount = 0;
				entries.Clear(0, clearCount);
			}
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific key.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key to locate in the dictionary.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsKey(TKey key)
		{
			return FindEntry(key) >= 0;
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific value.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="value">The value to locate in the dictionary.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsValue(TValue value)
		{
			var rawEntries = entries.ptr;
			for (var i = 0; i < count; i++)
			{
				if (rawEntries[i].hashCode >= 0 && value.Equals(rawEntries[i].value))
					return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindEntry(in TKey key)
		{
			if (buckets.IsCreated)
			{
				var hashCode = key.GetHashCode() & _hashCodeMask;
				var rawBuckets = buckets.ptr;
				var rawEntries = entries.ptr;

				for (var i = rawBuckets[hashCode % buckets.Length]; i >= 0; i = rawEntries[i].next)
				{
					if (rawEntries[i].hashCode == hashCode && rawEntries[i].key.Equals(key))
						return i;
				}
			}

			return -1;
		}

		private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
		{
			if (!buckets.IsCreated)
			{
				Initialize(default, 0);
			}

			var hashCode = key.GetHashCode() & _hashCodeMask;
			var targetBucket = hashCode % buckets.Length;

			var rawBuckets = buckets.ptr;
			var rawEntries = entries.ptr;

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
					IncreaseCapacity();
					targetBucket = hashCode % buckets.Length;

					rawBuckets = buckets.ptr;
					rawEntries = entries.ptr;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncreaseCapacity()
		{
			IncreaseCapacity(count.ExpandPrime());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncreaseCapacity(int newSize)
		{
			var bucketsArray = new UnsafeArray<int>(buckets.memoryId, newSize, ClearOptions.UninitializedMemory);
			bucketsArray.Fill(-1);
			var entryArray = new UnsafeArray<Entry>(entries.memoryId, newSize, ClearOptions.ClearMemory);

			MemoryExt.MemCopy(entries.ptr, entryArray.ptr, count);

			var rawBuckets = bucketsArray.ptr;
			var rawEntries = entryArray.ptr;

			for (var i = 0; i < count; i++)
			{
				if (rawEntries[i].hashCode >= 0)
				{
					var bucket = rawEntries[i].hashCode % newSize;
					rawEntries[i].next = rawBuckets[bucket];
					rawBuckets[bucket] = i;
				}
			}

			buckets.Dispose();
			entries.Dispose();

			buckets = bucketsArray;
			entries = entryArray;
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(TKey key)
		{
			return Remove(key, out _);
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="worldator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(TKey key, out TValue value)
		{
			value = default;

			if (!buckets.IsCreated)
				return false;

			var hashCode = key.GetHashCode() & _hashCodeMask;
			var bucket = hashCode % buckets.Length;
			var last = -1;

			var rawBuckets = buckets.ptr;
			var rawEntries = entries.ptr;

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryAdd(TKey key, TValue value)
		{
			return TryInsert(key, value, InsertionBehavior.None);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int EnsureCapacity(int capacity)
		{
			E.ASSERT(IsCreated);

			var num = entries.Length;
			if (num >= capacity)
			{
				return num;
			}

			if (!buckets.IsCreated)
			{
				Initialize(default, capacity);
				return Capacity;
			}

			var prime = capacity.GetPrime();
			IncreaseCapacity(prime);
			return prime;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator GetEnumerator()
		{
			return new Enumerator(GetEntryPtr(), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<SafePtr<Entry>> IEnumerable<SafePtr<Entry>>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private class EquatableDictionaryProxy
		{
			private UnsafeDictionary<TKey, TValue> _dictionary;

			public EquatableDictionaryProxy(UnsafeDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
			}

			public UnsafeArray<int> Buckets => _dictionary.buckets;
			public UnsafeArray<Entry> Entries => _dictionary.entries;
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
					var e = _dictionary.GetEnumerator();
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

		public struct Enumerator :
			IEnumerator<Entry>,
			IEnumerator<SafePtr<Entry>>
		{
			private readonly SafePtr<Entry> _entries;
			private readonly int _count;
			private int _index;

			[INLINE(MethodImplOptions.AggressiveInlining)]
			internal Enumerator(SafePtr<Entry> entries, int count)
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

			object IEnumerator.Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Current;
			}

			SafePtr<Entry> IEnumerator<SafePtr<Entry>>.Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _entries + _index;
			}

			public Entry Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => (_entries + _index).Value();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				this = default;
			}
		}
	}
}
