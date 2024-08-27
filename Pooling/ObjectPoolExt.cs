using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class Pooling
	{
		public static PooledObject<T> Get<T>(this IObjectPool<T> pool, out T obj)
		{
			obj = pool.Get();

			return new PooledObject<T>(pool, obj);
		}

		public static void ReturnToPool<T0, T1>(this Dictionary<T0, T1> dictionary)
		{
			DictionaryPool<T0, T1>.Release(dictionary);
		}

		public static void ReturnToPool<T>(this List<T> list)
		{
			ListPool<T>.Release(list);
		}

		public static void Get<T>(out List<T> list)
		{
			list = ListPool<T>.Get();
		}

		public static void ReturnToPool<T>(this HashSet<T> hashSet)
		{
			HashSetPool<T>.Release(hashSet);
		}

		public static void Get<T>(out HashSet<T> hashSet)
		{
			hashSet = HashSetPool<T>.Get();
		}
	}
}
