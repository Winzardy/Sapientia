using System.Collections;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	public sealed partial class HashMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
		where TKey : notnull
		where TValue : struct
	{
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (var (key, index) in _keyToIndex)
				yield return new KeyValuePair<TKey, TValue>(key, _values[index]);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
