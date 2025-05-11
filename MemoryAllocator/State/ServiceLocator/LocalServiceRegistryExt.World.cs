using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class LocalServiceRegistryExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetLocalService<T>(this World world, T service)
		{
			ServiceManagement.ServiceLocator<T>.SetService(world.worldId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveLocalService<T>(this World world, T service)
		{
			ServiceManagement.ServiceLocator<T>.RemoveService(world.worldId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetLocalService<T>(this World world)
		{
			return ServiceManagement.ServiceLocator<T>.GetService(world.worldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetLocalService<T>(this World world, out T service)
		{
			return ServiceManagement.ServiceLocator<T>.TryGetService(world.worldId, out service);
		}
	}
}
