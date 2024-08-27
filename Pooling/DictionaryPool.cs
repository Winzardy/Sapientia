using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class DictionaryPool<T0, T1>
	{
		static DictionaryPool()
		{
			if (!StaticObjectPool<Dictionary<T0, T1>>.IsInitialized)
				StaticObjectPool<Dictionary<T0, T1>>.Initialize(new(new Policy(), true));
		}

		public static Dictionary<T0, T1> Get() => StaticObjectPool<Dictionary<T0, T1>>.Get();

		public static PooledObject<Dictionary<T0, T1>> Get(out Dictionary<T0, T1> result) =>
			StaticObjectPool<Dictionary<T0, T1>>.Get(out result);

		public static void Release(Dictionary<T0, T1> obj) => StaticObjectPool<Dictionary<T0, T1>>.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<Dictionary<T0, T1>>
		{
			public override void OnRelease(Dictionary<T0, T1> dict) => dict.Clear();
		}
	}
}
