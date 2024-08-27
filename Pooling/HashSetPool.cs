using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class HashSetPool<T>
	{
		static HashSetPool()
		{
			if (!StaticObjectPool<HashSet<T>>.IsInitialized)
				StaticObjectPool<HashSet<T>>.Initialize(new(new Policy(), true));
		}

		public static HashSet<T> Get() => StaticObjectPool<HashSet<T>>.Get();

		public static PooledObject<HashSet<T>> Get(out HashSet<T> result) =>
			StaticObjectPool<HashSet<T>>.Get(out result);

		public static void Release(HashSet<T> obj) => StaticObjectPool<HashSet<T>>.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<HashSet<T>>
		{
			public override void OnRelease(HashSet<T> hashSet) => hashSet.Clear();
		}
	}
}
