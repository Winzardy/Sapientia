using System.Text;

namespace Sapientia.Pooling
{
	public static class StringBuilderPool
	{
		static StringBuilderPool() => StaticObjectPool.Initialize(new Policy());

		public static StringBuilder Get() => StaticObjectPool.Get<StringBuilder>();

		public static PooledObject<StringBuilder> Get(out StringBuilder result) => StaticObjectPool.Get(out result);

		public static void Release(StringBuilder obj) => StaticObjectPool.Release(obj);

		private class Policy : DefaultObjectPoolPolicy<StringBuilder>
		{
			public override void OnRelease(StringBuilder obj) => obj.Clear();
		}
	}
}
