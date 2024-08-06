using System;

namespace Sapientia.Pooling
{
    public struct PooledObject<T> : IDisposable
    {
        private readonly IObjectPool<T> _pool;
        private readonly T _obj;

        private bool _disposed;

        public PooledObject(IObjectPool<T> pool, T obj)
        {
            _pool = pool;
            _obj = obj;
            _disposed = false;
        }

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PooledObject<T>));

            _disposed = true;

            _pool.Release(_obj);
        }
    }
}