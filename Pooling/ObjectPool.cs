using System;
using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public class ObjectPool<T> : IObjectPool<T>, IDisposable
	{
		private readonly Queue<T> _items;
		private readonly IObjectPoolPolicy<T> _policy;

		private readonly bool _collectionCheck;
		private readonly int _maxSize;

		public ObjectPool(
			IObjectPoolPolicy<T> policy,
			bool collectionCheck = false,
			int capacity = 0,
			int maxSize = 0)
		{
			_items = capacity > 0 ? new Queue<T>(capacity) : new Queue<T>();
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
				_policy.OnDispose(_items.Dequeue());
			}
		}

		public T Get()
		{
			T item;

			if (_items.Count > 0)
			{
				item = _items.Dequeue();
			}
			else
			{
				item = _policy.Create();
			}

			_policy.OnGet(item);

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

			if (_maxSize > 0 && _items.Count >= _maxSize)
			{
				_policy.OnDispose(obj);
			}
			else
			{
				Push(obj);
			}
		}

		private void Push(T obj) => _items.Enqueue(obj);
	}
}
