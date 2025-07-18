using Sapientia.Collections;

namespace Sapientia.Pooling
{
	public static class HashMapPool<T0, T1>
		where T1 : struct
	{
		static HashMapPool() => StaticObjectPool.Initialize(new Policy());

		public static HashMap<T0, T1> Get() => StaticObjectPool.Get<HashMap<T0, T1>>();

		public static PooledObject<HashMap<T0, T1>> Get(out HashMap<T0, T1> result) =>
			StaticObjectPool.Get(out result);

		public static void Release(HashMap<T0, T1> obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<HashMap<T0, T1>>
		{
			public override void OnRelease(HashMap<T0, T1> dict) => dict.Clear();
		}
	}
}
