namespace Sapientia.Pooling
{
	public static class ObjectPoolExt
	{
		public static PooledObject<T> Get<T>(this IObjectPool<T> pool, out T obj)
		{
			obj = pool.Get();

			return new PooledObject<T>(pool, obj);
		}
	}
}
