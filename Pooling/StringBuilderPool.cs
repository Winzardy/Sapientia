using System.Text;

namespace Sapientia.Pooling
{
	public static class StringBuilderPool
	{
		private static ObjectPool<StringBuilder> _instance = new(new PoolPolicy());

		public static StringBuilder Get() => _instance.Get();

		public static PooledObject<StringBuilder> Get(out StringBuilder result) => _instance.Get(out result);

		public static void Release(StringBuilder stringBuilder) => _instance.Release(stringBuilder);

		class PoolPolicy : DefaultObjectPoolPolicy<StringBuilder>
		{
			public override void OnRelease(StringBuilder obj)
			{
				obj.Clear();
			}
		}
	}
}