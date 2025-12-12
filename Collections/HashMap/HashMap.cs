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
		private TValue _defaultValue;

		internal Dictionary<TKey, int> _keyToIndex;
		internal SimpleList<TValue> _values;

		private (TKey key, bool has) _last;

		public ref TValue this[TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (!Contains(key))
					return ref GetOrAdd(key);

				return ref _values[_keyToIndex[key]];
			}
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
			_defaultValue = default;

			_values = new();
			_keyToIndex = new Dictionary<TKey, int>();
		}

		public HashMap(HashMap<TKey, TValue> source)
		{
			_defaultValue = default;

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

		public HashMap(int capacity)
		{
			_defaultValue = default;

			_values = new(capacity);
			_keyToIndex = new Dictionary<TKey, int>(capacity);
		}

		public void Clear()
		{
			_keyToIndex.Clear();
			_values.Clear();

			_last = default;
		}

		public void SetOrAdd(TKey key, in TValue value)
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

		public void Remove(TKey key)
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

		public ref TValue GetOrAdd(TKey key, HashMapFactory<TValue>? factory = null)
		{
			if (!Contains(key))
				SetOrAdd(key, factory?.Invoke() ?? new TValue());

			return ref this[key];
		}

		public bool Contains(TKey key) => _keyToIndex.ContainsKey(key);

		public ref readonly TValue GetOrDefault(in TKey key)
		{
			if (Contains(key))
				return ref this[key];

			return ref _defaultValue;
		}

		public ref readonly TValue GetOrDefault(in TKey key, ref TValue defaultValue)
		{
			if (Contains(key))
				return ref this[key];

			return ref defaultValue;
		}

		public bool TryGetValue(in TKey key, out TValue value)
		{
			if (Contains(key))
			{
				value = this[key];
				return true;
			}

			value = default;
			return false;
		}

		public int GetIndexByKey(TKey key)
		{
			return _keyToIndex[key];
		}

		public TValue[] ToArray() => _values.ToArray();

		public bool IsEmpty() => _values.Count == 0;
	}
}
