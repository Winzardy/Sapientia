using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.Collections
{
	public delegate ref TValue HashMapFactory<TValue>();

	[Serializable]
	public sealed partial class HashMap<TKey, TValue>
		where TKey : notnull
		where TValue : struct
	{
		private TValue _defaultValue = default;

		internal Dictionary<TKey, int> _keyToIndex;
		internal SimpleList<TValue> _values;

		public ref TValue this[in TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _values[_keyToIndex[key]];
		}

		public ref TValue this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _values[index];
		}

		public int Count => _values.Count;

		public IEnumerable<TKey> Keys => _keyToIndex.Keys;

		public HashMap()
		{
			_values = new();
			_keyToIndex = new Dictionary<TKey, int>();
		}

		public HashMap(HashMap<TKey, TValue> source)
		{
			_values = new SimpleList<TValue>(source._values);
			_keyToIndex = new Dictionary<TKey, int>(source.Count);

			foreach (var (key, index) in source._keyToIndex)
				_keyToIndex[key] = index;
		}

		public void Clear()
		{
			_keyToIndex.Clear();
			_values.Clear();
		}

		public void Add(in TKey key, in TValue value)
		{
			var index = _values.Add(in value);
			_keyToIndex[key] = index;
		}

		public void Remove(in TKey key)
		{
			var index = _keyToIndex[key];
			_values.RemoveAt(index);
			_keyToIndex.Remove(key);
		}

		public ref TValue GetOrAdd(in TKey key, HashMapFactory<TValue>? factory = null)
		{
			if (!Contains(in key))
				Add(in key, factory?.Invoke() ?? new TValue());

			return ref this[key];
		}

		public ref TValue GetOrDefault(in TKey key)
		{
			if (Contains(in key))
				return ref this[in key];

			return ref _defaultValue;
		}

		public ref TValue GetOrDefault(in TKey key, ref TValue defaultValue)
		{
			if (Contains(in key))
				return ref this[key];

			return ref defaultValue;
		}

		public bool Contains(in TKey key) => _keyToIndex.ContainsKey(key);
		public bool Contains(int index) => _values.ContainsIndexSafe(index);

		public bool IsEmpty() => _values.Count == 0;
	}
}
