using System;
using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public class ObjectPool<T> : IObjectPool<T>, IDisposable
	{
		private readonly HashSet<T> _used;

		private readonly Queue<T> _queue;
		private readonly IObjectPoolPolicy<T> _policy;

		private readonly bool _collectionCheck;
		private readonly int _maxSize;

		public ObjectPool(
			IObjectPoolPolicy<T> policy,
			bool collectionCheck = false,
			int capacity = 0,
			int maxSize = 0)
		{
			_used = capacity > 0 ? new HashSet<T>(capacity) : new HashSet<T>();
			_queue = capacity > 0 ? new Queue<T>(capacity) : new Queue<T>();
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
			Clear(false);

			while (_queue.Count > 0)
				_policy.OnDispose(_queue.Dequeue());
		}

		public T Get()
		{
			T item;

			if (_queue.Count > 0)
			{
				item = _queue.Dequeue();
			}
			else
			{
				item = _policy.Create();
			}

			_policy.OnGet(item);
			_used.Add(item);

			return item;
		}

		public void Release(T obj) => Push(obj, true);

		public void Push(T obj, bool removeFromUsed, bool release = true)
		{
#if DEBUG
			if (_collectionCheck)
			{
				if (_queue.Contains(obj))
					throw new ArgumentException("The specified instance is already held by the pool.", nameof(obj));
			}
#endif
			if (release)
				_policy.OnRelease(obj);

			if (removeFromUsed)
				_used.Remove(obj);

			if (_maxSize > 0 && _queue.Count >= _maxSize)
			{
				_policy.OnDispose(obj);
			}
			else
			{
				Push(obj);
			}
		}

		public void Clear(bool release = true)
		{
			foreach (var obj in _used)
				Push(obj, false, release);

			_used.Clear();
		}

		private void Push(T obj) => _queue.Enqueue(obj);
	}
}
