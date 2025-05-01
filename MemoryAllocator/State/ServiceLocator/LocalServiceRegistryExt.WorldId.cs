using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public static partial class LocalServiceRegistryExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetLocalService<T>(this ref WorldId worldId, T service)
		{
			ServiceManagement.ServiceLocator<T>.SetService(worldId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveLocalService<T>(this ref WorldId worldId, T service)
		{
			ServiceManagement.ServiceLocator<T>.RemoveService(worldId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetLocalService<T>(this ref WorldId worldId)
		{
			return ServiceManagement.ServiceLocator<T>.GetService(worldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetLocalService<T>(this ref WorldId worldId, out T service)
		{
			return ServiceManagement.ServiceLocator<T>.TryGetService(worldId, out service);
		}
	}
}
