using Sapientia.Extensions;

namespace Sapientia.Pooling
{
	public sealed class StaticObjectPool<T> : StaticWrapper<ObjectPool<T>>
	{
		internal static T Get() => Instance.Get();

		internal static PooledObject<T> Get(out T result) => Instance.Get(out result);

		internal static void Release(T obj) => Instance.Release(obj);
	}

	public static class StaticObjectPool
	{
		internal static void Initialize<T>(IObjectPoolPolicy<T> policy, bool collectionCheck = true)
		{
			if (!StaticObjectPool<T>.IsInitialized)
				StaticObjectPool<T>.Initialize(new(policy, collectionCheck));
		}

		internal static PooledObject<T> Get<T>(out T result) => StaticObjectPool<T>.Get(out result);
		internal static T Get<T>() => StaticObjectPool<T>.Get();
		internal static void Release<T>(T obj) => StaticObjectPool<T>.Release(obj);
	}
}
