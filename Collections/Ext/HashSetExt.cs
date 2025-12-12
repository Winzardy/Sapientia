using System.Collections.Generic;

namespace Sapientia.Collections
{
	public static class HashSetExt
	{
		public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> enumerable)
		{
			if (enumerable == null)
				return;
			foreach (var value in enumerable)
				hashSet.Add(value);
		}
	}
}
