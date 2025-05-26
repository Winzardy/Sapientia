using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Pooling
{
	public sealed class StaticObjectPool<T> : StaticProvider<ObjectPool<T>>
		where T : class
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

		internal static T Get() => pool.Get();

		internal static PooledObject<T> Get(out T result) => pool.Get(out result);

		internal static void Release(T obj) => pool.Release(obj);
	}

	public static class StaticObjectPool
	{
		internal static void Initialize<T>(IObjectPoolPolicy<T> policy) where T : class
		{
			if (!StaticObjectPool<T>.IsInitialized)
				StaticObjectPool<T>.Initialize(new(policy));
		}

		internal static PooledObject<T> Get<T>(out T result) where T : class
			=> StaticObjectPool<T>.Get(out result);

		internal static T Get<T>() where T : class
			=> StaticObjectPool<T>.Get();

		internal static void Release<T>(T obj) where T : class
			=> StaticObjectPool<T>.Release(obj);
	}

	public static class StaticObjectPoolExtensions
	{
		public static void ReleaseToStaticPool<T>(this T obj) where T : class
			=> StaticObjectPool<T>.Release(obj);
	}
}
