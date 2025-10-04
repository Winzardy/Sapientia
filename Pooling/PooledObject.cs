using System;

namespace Sapientia.Pooling
{
	public struct PooledObject<T> : IDisposable
	{
		private readonly IObjectPool<T> _pool;
		private readonly T _obj;

		private bool _disposed;

		public T Obj => _obj;

		public PooledObject(IObjectPool<T> pool, T obj)
		{
			_pool = pool;
			_obj = obj;
			_disposed = false;
		}

		public void Dispose() => Release();

		public void ReleaseSafe()
		{
			if (_disposed)
				return;

			Dispose();
		}

		public void Release()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(PooledObject<T>));

			_disposed = true;

			_pool.Release(_obj);
		}

		public static void Release(ref PooledObject<T> p)
		{
			if (p._obj == null)
				return;

			p.Release();
			p = default;
		}

		public static implicit operator T(PooledObject<T> pooledObject) => pooledObject.Obj;
	}

	public static class PooledObjectUtility
	{
		public static void ReleaseAndSetNull<T>(ref PooledObject<T>? obj)
		{
			obj?.ReleaseSafe();
			obj = null;
		}
	}
}
