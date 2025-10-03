using System;
using System.Collections;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	public sealed partial class HashMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetPairEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetPairEnumerator();

		public IEnumerator<KeyValuePair<TKey, TValue>> GetPairEnumerator()
		{
			foreach (var (key, index) in _keyToIndex)
				yield return new KeyValuePair<TKey, TValue>(key, _values[index]);
		}

		public Enumerator GetEnumerator() => new(this);

		public ref struct Enumerator
		{
			private readonly SimpleList<TValue> _list;
			private int _index;

			public ref TValue Current => ref _list[_index];

			internal Enumerator(HashMap<TKey, TValue> map)
			{
				_list = map._values;
				_index = -1;
			}

			public bool MoveNext()
			{
				_index++;
				return _index < _list.Count;
			}

			public void Reset()
			{
				_index = -1;
			}
		}
	}

	public static class HashMapUtility
	{
		public static HashMap<TKey, TValue> ToHashMap<TKey, TValue>(this TValue[] array, HashMapKeySelector<TValue, TKey> keySelector)
			where TValue : struct
		{
			var map = new HashMap<TKey, TValue>(array.Length);
			foreach (ref var value in array.AsSpan())
				map.SetOrAdd(keySelector.Invoke(value), value);

			return map;
		}
	}

	public delegate TKey HashMapKeySelector<TValue, TKey>(in TValue value) where TValue : struct;
}
