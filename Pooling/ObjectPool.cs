#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Sapientia.Collections;

namespace Sapientia.Pooling
{
	// TODO: Динамическое capacity, например: сцена требует очень много звуков,
	// а следующая нет, можно было бы подумать чтобы для пула менять капасити по контексту))
	/// <summary>
	/// Default implementation of <see cref="ObjectPool{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type to pool objects for.</typeparam>
	/// <remarks>This implementation keeps a cache of retained objects. This means that if objects are returned when the pool has already reached "maximumRetained" objects they will be available to be Garbage Collected.</remarks>
	/// <seealso href="https://github.com/dotnet/aspnetcore/blob/ed74665e773dd1ebea3289c5662d71c590305932/src/ObjectPool/src/DefaultObjectPool.cs">DefaultObjectPool.cs</seealso>
	public class ObjectPool<T> : IObjectPool<T>, IDisposable
		where T : class
	{
		protected static readonly int DEFAULT_CAPACITY = Environment.ProcessorCount * 3;

		private readonly ConcurrentQueue<T> _queue;
		private readonly IObjectPoolPolicy<T> _policy;

		private readonly int _maxCapacity;

		private T? _single;
		private int _count;

		/// <summary>
		/// Creates an instance of <see cref="ObjectPool{T}"/>.
		/// </summary>
		/// <param name="policy">The pooling policy to use.</param>
		public ObjectPool(IObjectPoolPolicy<T> policy) : this(policy, DEFAULT_CAPACITY)
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="ObjectPool{T}"/>.
		/// </summary>
		/// <param name="policy">The pooling policy to use.</param>
		/// <param name="maximumRetained">The maximum number of objects to retain in the pool. <c>-1</c> - infinity</param>
		public ObjectPool(IObjectPoolPolicy<T> policy, int maximumRetained)
		{
			_policy = policy ?? throw new ArgumentNullException(nameof(policy));
			_queue = new ConcurrentQueue<T>();
			_maxCapacity = maximumRetained - 1; // -1 to account for _single
		}

		public void Dispose() => Clear();

		public T Get()
		{
			var item = _single;
			if (item == null || Interlocked.CompareExchange(ref _single, null, item) != item)
			{
				if (_queue.TryDequeue(out item))
					Interlocked.Decrement(ref _count);
				else
					item = _policy.Create();
			}

			_policy.OnGet(item);
			return item;
		}

		public void Release(T obj)
		{
#if DebugLog
			if (ReferenceEquals(_single, obj) || _queue.ReferenceContains(obj))
				throw new ArgumentException("Already in pool", nameof(obj));
#endif

			if (_single != null || Interlocked.CompareExchange(ref _single, obj, null) != null)
			{
				if (Interlocked.Increment(ref _count) <= _maxCapacity || _maxCapacity < 0)
				{
					_queue.Enqueue(obj);
				}
				else
				{
					Interlocked.Decrement(ref _count);
					_policy.OnDispose(obj);
					return;
				}
			}

			_policy.OnRelease(obj);
		}

		public void Clear()
		{
			var fast = Interlocked.Exchange(ref _single, null);
			if (fast != null)
				_policy.OnDispose(fast);

			while (_queue.TryDequeue(out var obj))
			{
				Interlocked.Decrement(ref _count);
				_policy.OnDispose(obj);
			}
		}
	}

	public static class ObjectPoolExtensions
	{
		public static PooledObject<T> Get<T>(this IObjectPool<T> pool, out T obj)
		{
			obj = pool.Get();
			return new PooledObject<T>(pool, obj);
		}

		public static void Release<T>(this ObjectPool<T> pool, ICollection<T> collection)
			where T : class =>
			collection.ReleaseAndClear(pool);

		public static void ReleaseAndClear<T>(this ICollection<T> collection, ObjectPool<T> pool)
			where T : class
		{
			foreach (var obj in collection)
				pool.Release(obj);

			collection.Clear();
		}
	}
}
