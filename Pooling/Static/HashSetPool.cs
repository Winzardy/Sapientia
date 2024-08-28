using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class HashSetPool<T>
	{
		static HashSetPool() => StaticObjectPool.Initialize(new Policy());

		public static HashSet<T> Get() => StaticObjectPool.Get<HashSet<T>>();

		public static PooledObject<HashSet<T>> Get(out HashSet<T> result) =>
			StaticObjectPool.Get(out result);

		public static void Release(HashSet<T> obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<HashSet<T>>
		{
			public override void OnRelease(HashSet<T> hashSet) => hashSet.Clear();
		}


	}
}
