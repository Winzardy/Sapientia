using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.Collections
{
	// WIP
	/*public interface IKey
	{
		public int DictionaryId { get; set; }
	}

	public struct Key : IKey
	{
		public int DictionaryId { get; set; }
	}

	public struct KeyContainer<T> : IKey
	{
		public int DictionaryId { get; set; }
		public T key;
	}

	public struct KeyValuePair<TKey, TValue> where TKey : IKey
	{
		public TKey key;
		public TValue value;
	}

	public class FastDictionary<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>> where TKey : IKey, IEquatable<TKey>
	{
		private readonly IndexAllocSparseSet<KeyValuePair<TKey, TValue>> _sparseSet;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseSet.Count;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseSet.Capacity;
		}

		public ref readonly KeyValuePair<TKey, TValue>[] GetKeyValueArray() => ref _sparseSet.GetValueArray();

		public ref readonly IndexAllocSparseSet<KeyValuePair<TKey, TValue>> GetSparseSet() => ref _sparseSet;

		public FastDictionary(int capacity = 8)
		{
			_sparseSet = new IndexAllocSparseSet<KeyValuePair<TKey, TValue>>(capacity);
		}

		public TValue this[in TKey key]
		{
			get
			{
				if (TryGetValue(key, out var value))
					return value;
				return default;
			}
			set
			{
				if (!ContainsKey(key))
					throw new Exception("Key doesn't contains in the dictionary");
				Insert(key, value);
			}
		}

		public void Set<T>(T key, in TValue value) where T : class, TKey
		{
			if (!ContainsKey(key))
				key.DictionaryId = _sparseSet.AllocateIndexId();
			Insert(key, value);
		}

		public void Set<T>(ref T key, in TValue value) where T : struct, TKey
		{
			if (!ContainsKey(key))
				key.DictionaryId = _sparseSet.AllocateIndexId();
			Insert(key, value);
		}

		public void Add<T>(T key, in TValue value) where T : class, TKey
		{
			if (ContainsKey(key))
				throw new Exception("An item with the same key has already been added");

			key.DictionaryId = _sparseSet.AllocateIndexId();
			Insert(key, value);
		}

		public void Add<T>(ref T key, in TValue value) where T : struct, TKey
		{
			if (ContainsKey(key))
				throw new Exception("An item with the same key has already been added");

			key.DictionaryId = _sparseSet.AllocateIndexId();
			Insert(key, value);
		}

		public bool TryAdd<T>(T key, in TValue value) where T : class, TKey
		{
			if (!CanInsert(key))
				return false;

			key.DictionaryId = _sparseSet.AllocateIndexId();
			Insert(key, value);

			return true;
		}

		public bool TryAdd<T>(ref T key, in TValue value) where T : struct, TKey
		{
			if (!CanInsert(key))
				return false;

			key.DictionaryId = _sparseSet.AllocateIndexId();
			Insert(key, value);

			return true;
		}

		private bool CanInsert(in TKey key)
		{
			if (key == null)
				return false;
			if (_sparseSet.HasIndexId(key.DictionaryId))
			{
				ref var keyValue = ref _sparseSet.GetValue(key.DictionaryId);
				if (keyValue.key.Equals(key))
					return false;
			}

			return true;
		}

		private void Insert(in TKey key, in TValue value)
		{
			ref var keyValue = ref _sparseSet.GetValue(key.DictionaryId);
			keyValue.key = key;
			keyValue.value = value;
		}

		public bool TryGetValue(in TKey key, out TValue value)
		{
			if (key == null)
				throw new NullReferenceException("Key can not be null");
			if (!_sparseSet.HasIndexId(key.DictionaryId))
			{
				value = default;
				return false;
			}
			ref var keyValue = ref _sparseSet.GetValue(key.DictionaryId);
			if (!keyValue.key.Equals(key))
			{
				value = default;
				return false;
			}

			value = keyValue.value;
			return true;
		}

		public bool ContainsKey(in TKey key)
		{
			if (key == null)
				throw new NullReferenceException("Key can not be null");
			if (!_sparseSet.HasIndexId(key.DictionaryId))
				return false;

			ref var keyValue = ref _sparseSet.GetValue(key.DictionaryId);
			return keyValue.key.Equals(key);
		}

		public bool Remove(in TKey key, bool clear = false)
		{
			return Remove(key, out _, clear);
		}

		public bool Remove(in TKey key, out TValue value, bool clear = false)
		{
			if (TryGetValue(key, out value))
			{
				_sparseSet.ReleaseIndexId(key.DictionaryId, clear);
				return true;
			}
			return false;
		}

		public void Clear()
		{
			_sparseSet.Clear();
		}

		public void ClearFast()
		{
			_sparseSet.ClearFast();
		}

		public void Dispose()
		{
			_sparseSet.Dispose();
		}

		public void Dispose(bool clearArray)
		{
			_sparseSet.Dispose(clearArray);
		}

		public IndexAllocSparseSet<KeyValuePair<TKey, TValue>>.Enumerator GetEnumerator()
		{
			return _sparseSet.GetEnumerator();
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}*/
}