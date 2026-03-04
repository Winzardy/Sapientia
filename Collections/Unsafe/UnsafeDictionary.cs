using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.Collections
{
	public enum InsertionBehavior
	{
		none,
		overwriteExisting,
		throwOnExisting,
	}

	[DebuggerTypeProxy(typeof(UnsafeDictionary<,>.EquatableDictionaryProxy))]
	public struct UnsafeDictionary<TKey, TValue> : IDisposable
		where TKey : unmanaged, IEquatable<TKey>
		where TValue : unmanaged
	{
		public struct Entry
		{
			internal int hashCode; // Lower 31 bits of hash code, -1 if unused
			internal int next; // Index of next entry, -1 if last
			public TKey key; // Key of entry
			public TValue value; // Value of entry
		}

		private const int _hashCodeMask = 0x7FFFFFFF;

		private UnsafeArray<int> _buckets;
		private UnsafeArray<Entry> _entries;
		private int _count;
		private int _freeList;
		private int _freeCount;

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _buckets.IsCreated;
		}

		public readonly int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _count - _freeCount;
		}

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _buckets.Length;
		}

		public readonly int LastIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _count;
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
			_freeList = -1;
			_buckets = new UnsafeArray<int>(memoryId, prime);
			_buckets.Fill(-1);
			_entries = new UnsafeArray<Entry>(memoryId, prime);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_buckets.Dispose();
			_entries.Dispose();
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<Entry> GetEntryPtr()
		{
			return _entries.ptr;
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

			_buckets.CopyFrom(other._buckets);
			_entries.CopyFrom(other._entries);

			_count = other._count;
			_freeCount = other._freeCount;
			_freeList = other._freeList;
		}

		/// <summary><para>Gets or sets the value associated with the specified key.</para></summary>
		/// <param name="key">The key whose value is to be gotten or set.</param>
		public ref TValue this[TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				var entry = FindEntry(key);
				if (entry >= 0)
				{
					return ref _entries[entry].value;
				}

				throw new KeyNotFoundException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TValue GetValue(TKey key, out bool success)
		{
			var entry = FindEntry(key);
			if (entry >= 0)
			{
				success = true;
				return ref _entries[entry].value;
			}

			success = false;
			if (!_buckets.IsCreated)
			{
				Initialize(default, 0);
			}
			return ref _entries[0].value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(TKey key, out TValue value)
		{
			var entry = FindEntry(key);
			if (entry >= 0)
			{
				value = _entries[entry].value;
				return true;
			}

			value = UnsafeExt.DefaultRefReadonly<TValue>();
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TValue GetOrCreateValue(TKey key, TValue defaultValue = default)
		{
			var entry = FindEntry(key);
			if (entry >= 0)
			{
				return ref _entries[entry].value;
			}

			Add(key, defaultValue);
			return ref this[key];
		}

		/// <summary><para>Adds an element with the specified key and value to the dictionary.</para></summary>
		/// <param name="worldState"></param>
		/// <param name="key">The key of the element to add to the dictionary.</param>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(TKey key, TValue value)
		{
			TryInsert(key, value, InsertionBehavior.throwOnExisting);
		}

		/// <summary><para>Removes all elements from the dictionary.</para></summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			var clearCount = _count;
			if (clearCount > 0)
			{
				_buckets.Fill(-1);
				_count = 0;
				_freeList = -1;
				_freeCount = 0;
				_entries.Clear(0, clearCount);
			}
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific key.</para></summary>
		/// <param name="worldState"></param>
		/// <param name="key">The key to locate in the dictionary.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsKey(TKey key)
		{
			return FindEntry(key) >= 0;
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific value.</para></summary>
		/// <param name="worldState"></param>
		/// <param name="value">The value to locate in the dictionary.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsValue(TValue value)
		{
			var rawEntries = _entries.ptr;
			for (var i = 0; i < _count; i++)
			{
				if (rawEntries[i].hashCode >= 0 && value.Equals(rawEntries[i].value))
					return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindEntry(in TKey key)
		{
			if (_buckets.IsCreated)
			{
				var hashCode = key.GetHashCode() & _hashCodeMask;
				var rawBuckets = _buckets.ptr;
				var rawEntries = _entries.ptr;

				for (var i = rawBuckets[hashCode % _buckets.Length]; i >= 0; i = rawEntries[i].next)
				{
					if (rawEntries[i].hashCode == hashCode && rawEntries[i].key.Equals(key))
						return i;
				}
			}

			return -1;
		}

		private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
		{
			if (!_buckets.IsCreated)
			{
				Initialize(default, 0);
			}

			var hashCode = key.GetHashCode() & _hashCodeMask;
			var targetBucket = hashCode % _buckets.Length;

			var rawBuckets = _buckets.ptr;
			var rawEntries = _entries.ptr;

			for (var i = rawBuckets[targetBucket]; i >= 0; i = rawEntries[i].next)
			{
				if (rawEntries[i].hashCode == hashCode && rawEntries[i].key.Equals(key))
				{
					switch (behavior)
					{
						case InsertionBehavior.none:
							break;
						case InsertionBehavior.overwriteExisting:
							rawEntries[i].value = value;
							return true;
						case InsertionBehavior.throwOnExisting:
							E.ADDING_DUPLICATE();
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(behavior), behavior, null);
					}

					return false;
				}
			}

			int index;
			if (_freeCount > 0)
			{
				index = _freeList;
				_freeList = rawEntries[index].next;
				_freeCount--;
			}
			else
			{
				if (_count == _entries.Length)
				{
					IncreaseCapacity();
					targetBucket = hashCode % _buckets.Length;

					rawBuckets = _buckets.ptr;
					rawEntries = _entries.ptr;
				}

				index = _count;
				_count++;
			}

			rawEntries[index].hashCode = hashCode;
			rawEntries[index].next = _buckets[targetBucket];
			rawEntries[index].key = key;
			rawEntries[index].value = value;
			rawBuckets[targetBucket] = index;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncreaseCapacity()
		{
			IncreaseCapacity(_count.ExpandPrime());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncreaseCapacity(int newSize)
		{
			var bucketsArray = new UnsafeArray<int>(_buckets.memoryId, newSize, ClearOptions.UninitializedMemory);
			bucketsArray.Fill(-1);
			var entryArray = new UnsafeArray<Entry>(_entries.memoryId, newSize, ClearOptions.ClearMemory);

			MemoryExt.MemCopy(_entries.ptr, entryArray.ptr, _count);

			var rawBuckets = bucketsArray.ptr;
			var rawEntries = entryArray.ptr;

			for (var i = 0; i < _count; i++)
			{
				if (rawEntries[i].hashCode >= 0)
				{
					var bucket = rawEntries[i].hashCode % newSize;
					rawEntries[i].next = rawBuckets[bucket];
					rawBuckets[bucket] = i;
				}
			}

			_buckets.Dispose();
			_entries.Dispose();

			_buckets = bucketsArray;
			_entries = entryArray;
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="worldState"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(TKey key)
		{
			return Remove(key, out _);
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="worldState"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(TKey key, out TValue value)
		{
			value = default;

			if (!_buckets.IsCreated)
				return false;

			var hashCode = key.GetHashCode() & _hashCodeMask;
			var bucket = hashCode % _buckets.Length;
			var last = -1;

			var rawBuckets = _buckets.ptr;
			var rawEntries = _entries.ptr;

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
						_entries[last].next = rawEntries[i].next;
					}

					value = rawEntries[i].value;

					rawEntries[i].hashCode = -1;
					rawEntries[i].next = _freeList;
					rawEntries[i].key = default(TKey);
					rawEntries[i].value = default(TValue);

					_freeList = i;
					_freeCount++;

					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryAdd(TKey key, TValue value)
		{
			return TryInsert(key, value, InsertionBehavior.none);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int EnsureCapacity(int capacity)
		{
			E.ASSERT(IsCreated);

			var num = _entries.Length;
			if (num >= capacity)
			{
				return num;
			}

			if (!_buckets.IsCreated)
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

		private class EquatableDictionaryProxy
		{
			private UnsafeDictionary<TKey, TValue> _dictionary;

			public EquatableDictionaryProxy(UnsafeDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
			}

			public UnsafeArray<int> Buckets => _dictionary._buckets;
			public UnsafeArray<Entry> Entries => _dictionary._entries;
			public int Count => _dictionary._count;
			public int FreeList => _dictionary._freeList;
			public int FreeCount => _dictionary._freeCount;

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

		public struct Enumerator
		{
			private readonly SafePtr<Entry> _entries;
			private readonly int _count;
			private int _index;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

			public ref Entry Current
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
