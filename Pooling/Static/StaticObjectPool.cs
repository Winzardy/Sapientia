using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Pooling
{
	public sealed class StaticObjectPool<T> : StaticWrapper<ObjectPool<T>>
	{
		private static ObjectPool<T> pool
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance;
		}

		public static bool IsInitialized
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance != null;
		}

		internal static T Get()
		{
#if !CLIENT
			lock (pool)
#endif
				return pool.Get();
		}

		internal static PooledObject<T> Get(out T result)
		{
#if !CLIENT
			lock (pool)
#endif
				return pool.Get(out result);
		}

		internal static void Release(T obj)
		{
#if !CLIENT
			lock (pool)
#endif
				pool.Release(obj);
		}
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
