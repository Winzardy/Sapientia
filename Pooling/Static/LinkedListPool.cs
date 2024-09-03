using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class LinkedListPool<T>
	{
		static LinkedListPool() => StaticObjectPool.Initialize(new Policy());

		public static LinkedList<T> Get() => StaticObjectPool.Get<LinkedList<T>>();

		public static PooledObject<LinkedList<T>> Get(out LinkedList<T> result) => StaticObjectPool.Get(out result);

		public static void Release(LinkedList<T> obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<LinkedList<T>>
		{
			public override void OnRelease(LinkedList<T> list) => list.Clear();
		}
	}
}
