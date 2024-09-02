using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class StackPool<T>
	{
		static StackPool() => StaticObjectPool.Initialize(new Policy());

		public static Stack<T> Get() => StaticObjectPool.Get<Stack<T>>();

		public static PooledObject<Stack<T>> Get(out Stack<T> result) => StaticObjectPool.Get(out result);

		public static void Release(Stack<T> obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<Stack<T>>
		{
			public override void OnRelease(Stack<T> list) => list.Clear();
		}
	}
}
