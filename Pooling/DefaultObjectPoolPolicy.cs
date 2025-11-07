using System;

namespace Sapientia.Pooling
{
	public class DefaultObjectPoolPolicy<T> : IObjectPoolPolicy<T>
		where T : new()
	{
		public virtual T Create() => new();

		public virtual void OnGet(T obj)
		{
			if(obj is IPoolable poolable)
				poolable.OnGet();
		}

		public virtual void OnRelease(T obj)
		{
			if(obj is IPoolable poolable)
				poolable.Release();
		}

		public virtual void OnDispose(T obj)
		{
			if (obj is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}
}
