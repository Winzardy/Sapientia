using System;

namespace Sapientia.Pooling
{
	public class DefaultObjectPoolPolicy<T> : IObjectPoolPolicy<T>
		where T : new()
	{
		public virtual T Create() => new();

		public virtual void OnGet(T obj)
		{
		}

		public virtual void OnRelease(T obj)
		{
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
