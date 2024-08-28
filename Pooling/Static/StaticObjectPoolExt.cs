namespace Sapientia.Pooling
{
	public static class StaticObjectPoolExt
	{
		public static void ReleaseToStaticPool<T>(this T obj) => StaticObjectPool<T>.Release(obj);
	}
}
