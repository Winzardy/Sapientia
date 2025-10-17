using System.Collections.Concurrent;

namespace Sapientia.Pooling.Concurrent
{
	public static class ConcurrentDictionaryPool<T0, T1>
	{
		static ConcurrentDictionaryPool() => StaticObjectPool.Initialize(new Policy());

		public static ConcurrentDictionary<T0, T1> Get() => StaticObjectPool.Get<ConcurrentDictionary<T0, T1>>();

		public static PooledObject<ConcurrentDictionary<T0, T1>> Get(out ConcurrentDictionary<T0, T1> result) =>
			StaticObjectPool.Get(out result);

		public static void Release(ConcurrentDictionary<T0, T1> obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<ConcurrentDictionary<T0, T1>>
		{
			public override void OnRelease(ConcurrentDictionary<T0, T1> dict) => dict.Clear();
		}
	}
}
