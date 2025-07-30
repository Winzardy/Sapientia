using Sapientia.Collections;

namespace Sapientia.Pooling
{
	public static class ArrayPool<T>
	{
		static ArrayPool() => StaticObjectPool.Initialize(new Policy());

		public static Array<T> Get(int minimumLength)
		{
			var array = StaticObjectPool.Get<Array<T>>();
			array.Initialize(minimumLength);
			return array;
		}

		public static PooledObject<Array<T>> Get(out Array<T> result, int minimumLength)
		{
			var pooledObject = StaticObjectPool<Array<T>>.Get(out result);
			result.Initialize(minimumLength);
			return pooledObject;
		}

		public static void Release(Array<T> obj) => StaticObjectPool.Release(obj);

		private class Policy : IObjectPoolPolicy<Array<T>>
		{
			public void OnRelease(Array<T> array) => array.Clear();

			public Array<T> Create() => new();

			public void OnGet(Array<T> array)
			{
			}

			public void OnDispose(Array<T> array) => array.Dispose();
		}
	}
}
