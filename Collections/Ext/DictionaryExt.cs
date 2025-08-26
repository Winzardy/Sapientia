using System;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#efe20baff23f4f768efbfcb4e3282747
	/// </summary>
	public static class DictionaryExt
	{
		public static TValue GetOrCreateValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> createValueFunction)
		{
			if (!dict.TryGetValue(key, out var value))
			{
				value = createValueFunction.Invoke();
				dict[key] = value;
			}

			return value;
		}

		public static TValue GetOrCreateValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
		{
			if (!dict.TryGetValue(key, out var value))
			{
				value = new TValue();
				dict[key] = value;
			}

			return value;
		}

		public static bool IsDictionaryEqual<TKey, TValue>(this Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b)
		{
			if (a.Equals(b))
				return true;
			if (b == null || a.Count != b.Count || a.GetType() != b.GetType())
				return false;
			using var aEnumerator = a.GetEnumerator();
			using var bEnumerator = b.GetEnumerator();

			while (aEnumerator.MoveNext() && bEnumerator.MoveNext())
			{
				if (aEnumerator.Current.Key is IEquatable<TKey> equatableKey)
				{
					if (!equatableKey.Equals(bEnumerator.Current.Key))
						return false;
				}
				else if (!aEnumerator.Current.Key.Equals(bEnumerator.Current.Key))
					return false;

				if (aEnumerator.Current.Value is IEquatable<TValue> equatableValue)
				{
					if (!equatableValue.Equals(bEnumerator.Current.Value))
						return false;
				}
				else if (!aEnumerator.Current.Value.Equals(bEnumerator.Current.Value))
					return false;
			}

			return true;
		}

		public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> source)
			where TValue : ICloneable<TValue>
		{
			var dictionary = new Dictionary<TKey, TValue>(source.Count);
			foreach (var (key, pair) in source)
				dictionary.Add(key, pair.Clone());
			return dictionary;
		}

		public static TValue GetValueOrDefaultSafe<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue @default = default)
		{
			if (key == null)
				return @default;

			return dictionary.GetValueOrDefault(key, @default);
		}
	}
}
