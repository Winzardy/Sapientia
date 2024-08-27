using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class ListPool<T>
	{
		static ListPool() => StaticObjectPool.Initialize(new Policy());

		public static List<T> Get() => StaticObjectPool.Get<List<T>>();

		public static PooledObject<List<T>> Get(out List<T> result) => StaticObjectPool.Get(out result);

		public static void Release(List<T> obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<List<T>>
		{
			public override void OnRelease(List<T> list) => list.Clear();
		}
	}
}
