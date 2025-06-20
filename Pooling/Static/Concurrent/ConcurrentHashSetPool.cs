using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Sapientia.Pooling.Concurrent
{
	public static class ConcurrentHashSetPool<T>
	{
		static ConcurrentHashSetPool() => StaticObjectPool.Initialize(new Policy());

		public static ConcurrentHashSet<T> Get() => StaticObjectPool.Get<ConcurrentHashSet<T>>();

		public static PooledObject<ConcurrentHashSet<T>> Get(out ConcurrentHashSet<T> result) =>
			StaticObjectPool.Get(out result);

		public static void Release(ConcurrentHashSet<T> obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<ConcurrentHashSet<T>>
		{
			public override void OnRelease(ConcurrentHashSet<T> hashSet) => hashSet.Clear();
		}
	}

	//TODO: шок оказывается такого нет, чат гпт сказал это индастриал стандарт
	public class ConcurrentHashSet<T> : IEnumerable<T>
	{
		private readonly ConcurrentDictionary<T, byte> _dict = new();

		public bool Add(T item) => _dict.TryAdd(item, 0);

		public bool Remove(T item) => _dict.TryRemove(item, out _);

		public bool Contains(T item) => _dict.ContainsKey(item);

		public void Clear() => _dict.Clear();

		public IEnumerator<T> GetEnumerator() => _dict.Keys.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
