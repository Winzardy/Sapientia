using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	public static class DictionaryExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDictionaryEqual<TKey, TValue>(this Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b)
		{
			if (a.Equals(b))
				return true;
			if (b == null || a.Count != b.Count)
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
	}
}