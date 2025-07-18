using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.Collections
{
	public delegate ref TValue HashMapFactory<TValue>();

	[Serializable]
	public sealed partial class HashMap<TKey, TValue>
		where TValue : struct
	{
		private TValue _defaultValue = default;

		internal Dictionary<TKey, int> _keyToIndex;
		internal SimpleList<TValue> _values;

		private (TKey key, bool has) _last;

		public ref TValue this[in TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _values[_keyToIndex[key]];
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
			{
				_keyToIndex[key] = index;

				if (index != _values.Count - 1)
					continue;

				_last = (key, true);
			}
		}

		public void Clear()
		{
			_keyToIndex.Clear();
			_values.Clear();

			_last = default;
		}

		public void SetOrAdd(in TKey key, in TValue value)
		{
			if (_keyToIndex.TryGetValue(key, out var index))
			{
				_values[index] = value;
			}
			else
			{
				index = _values.Add(in value);
				_last = (key, true);
				_keyToIndex[key] = index;
			}
		}

		public void Remove(in TKey key)
		{
			if (!_keyToIndex.TryGetValue(key, out var index))
				return;

			var lastIndex = _keyToIndex[_last.key];

			if (index != lastIndex)
			{
				_values[index] = _values[lastIndex];
				_keyToIndex[_last.key] = index;
			}

			_last.has = false;
			_values.RemoveAt(lastIndex);
			_keyToIndex.Remove(key);

			if (_values.Count <= 0)
				return;

			foreach (var (k, i) in _keyToIndex)
			{
				if (i != _values.Count - 1)
					continue;

				_last = (k, true);
				break;
			}
		}

		public ref TValue GetOrAdd(in TKey key, HashMapFactory<TValue>? factory = null)
		{
			if (!Contains(in key))
				SetOrAdd(in key, factory?.Invoke() ?? new TValue());

			return ref this[key];
		}

		public bool Contains(in TKey key) => _keyToIndex.ContainsKey(key);

		public ref readonly TValue GetOrDefault(in TKey key)
		{
			if (Contains(in key))
				return ref this[in key];

			return ref _defaultValue;
		}

		public ref readonly TValue GetOrDefault(in TKey key, ref TValue defaultValue)
		{
			if (Contains(in key))
				return ref this[key];

			return ref defaultValue;
		}

		public bool IsEmpty() => _values.Count == 0;
	}
}
