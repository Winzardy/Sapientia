using System;
using System.Collections.Generic;

namespace Sapientia.Pooling
{
    public class ObjectPool<T> : IObjectPool<T>, IDisposable
    {
        private readonly Stack<T> _items;
        private readonly IObjectPoolPolicy<T> _policy;

        private readonly bool _collectionCheck;
        private readonly int _maxSize;

        private int _numActiveObjects;
        private int _numInactiveObjects;

        public ObjectPool(
            IObjectPoolPolicy<T> policy,
            bool collectionCheck = false,
            int capacity = 0,
            int maxSize = 0)
        {
            _items = capacity > 0 ? new Stack<T>(capacity) : new Stack<T>();
            _policy = policy;
            _collectionCheck = collectionCheck;
            _maxSize = maxSize;

            for (int i = 0; i < capacity; i++)
            {
                var obj = _policy.Create();
                _policy.OnRelease(obj);
                Push(obj);
            }
        }

        public void Dispose()
        {
            while (_items.Count > 0)
            {
                _policy.OnDispose(_items.Pop());
            }
        }

        public T Get()
        {
            T item;

            if (_items.Count > 0)
            {
                item = _items.Pop();

                _numInactiveObjects--;
            }
            else
            {
                item = _policy.Create();
            }

            _policy.OnGet(item);

            _numActiveObjects++;

            return item;
        }

        public void Release(T obj)
        {
#if DEBUG
            if (_collectionCheck)
            {
                if (_items.Contains(obj))
                    throw new ArgumentException("The specified instance is already held by the pool.", nameof(obj));
            }
#endif
            _policy.OnRelease(obj);

            _numActiveObjects--;

            if (_maxSize > 0 && _items.Count >= _maxSize)
            {
                _policy.OnDispose(obj);
            }
            else
            {
                Push(obj);
            }
        }

        private void Push(T obj)
        {
            _items.Push(obj);

            _numInactiveObjects++;
        }
    }
}