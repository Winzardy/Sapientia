namespace Sapientia.Pooling
{
	public abstract class StaticObjectPool<T> : StaticWrapper<ObjectPool<T>>
	{
		public static T Get() => instance.Get();

		public static PooledObject<T> Get(out T result) => instance.Get(out result);

		public static void Release(T obj) => instance.Release(obj);
	}
}
