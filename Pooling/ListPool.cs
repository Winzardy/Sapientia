using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class ListPool<T>
	{
		static ListPool()
		{
			if (!StaticObjectPool<List<T>>.IsInitialized)
				StaticObjectPool<List<T>>.Initialize(new(new Policy(), true));
		}

		public static List<T> Get() => StaticObjectPool<List<T>>.Get();

		public static PooledObject<List<T>> Get(out List<T> result) =>
			StaticObjectPool<List<T>>.Get(out result);

		public static void Release(List<T> obj) => StaticObjectPool<List<T>>.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<List<T>>
		{
			public override void OnRelease(List<T> list) => list.Clear();
		}
	}
}
