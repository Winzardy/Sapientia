using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class PoolingExt
	{
		public static void ReturnToPool<T0, T1>(this Dictionary<T0, T1> dictionary)
		{
			DictionaryPool<T0, T1>.Release(dictionary);
		}

		public static void ReturnToPool<T>(this List<T> list)
		{
			ListPool<T>.Release(list);
		}

		public static void ReturnToPool<T>(this HashSet<T> list)
		{
			HashSetPool<T>.Release(list);
		}
	}
}
