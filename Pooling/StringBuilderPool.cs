using System.Text;

namespace Sapientia.Pooling
{
	public class StringBuilderPool
	{
		static StringBuilderPool()
		{
			if (!StaticObjectPool<StringBuilder>.IsInitialized)
				StaticObjectPool<StringBuilder>.Initialize(new(new Policy(), true));
		}

		public static StringBuilder Get() => StaticObjectPool<StringBuilder>.Get();

		public static PooledObject<StringBuilder> Get(out StringBuilder result) =>
			StaticObjectPool<StringBuilder>.Get(out result);

		public static void Release(StringBuilder obj) => StaticObjectPool<StringBuilder>.Release(obj);

		class Policy : DefaultObjectPoolPolicy<StringBuilder>
		{
			public override void OnRelease(StringBuilder obj) => obj.Clear();
		}
	}
}
