namespace Sapientia.Pooling
{
	public interface IPoolable
	{
		// TODO: пытался переименовать в OnRelease, не получилось, надо позже повторить...
		public void Release();

		public void OnGet()
		{
		}
	}

	public static class Pool<T>
		where T : class, IPoolable, new()
	{
		static Pool() => StaticObjectPool.Initialize(new Policy());

		public static T Get()
		{
			var obj = StaticObjectPool.Get<T>();
			obj.OnGet();
			return obj;
		}

		public static PooledObject<T> Get(out T result)
		{
			var pooledObject = StaticObjectPool.Get(out result);
			pooledObject.Obj.OnGet();
			return pooledObject;
		}

		public static void Release(T obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<T>
		{
			public override void OnRelease(T obj) => obj.Release();
		}
	}
}
