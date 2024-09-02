using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class QueuePool<T>
	{
		static QueuePool() => StaticObjectPool.Initialize(new Policy());

		public static Queue<T> Get() => StaticObjectPool.Get<Queue<T>>();

		public static PooledObject<Queue<T>> Get(out Queue<T> result) => StaticObjectPool.Get(out result);

		public static void Release(Queue<T> obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<Queue<T>>
		{
			public override void OnRelease(Queue<T> list) => list.Clear();
		}
	}
}
