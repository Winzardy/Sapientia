using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class DictionaryPool<T0, T1>
	{
		static DictionaryPool() => StaticObjectPool.Initialize(new Policy());

		public static Dictionary<T0, T1> Get() => StaticObjectPool.Get<Dictionary<T0, T1>>();

		public static PooledObject<Dictionary<T0, T1>> Get(out Dictionary<T0, T1> result) =>
			StaticObjectPool.Get(out result);

		public static void Release(Dictionary<T0, T1> obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<Dictionary<T0, T1>>
		{
			public override void OnRelease(Dictionary<T0, T1> dict) => dict.Clear();
		}
	}
}
