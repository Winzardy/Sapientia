using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class LocalServiceRegistryExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetLocalService<T>(this ref AllocatorId allocatorId, T service)
		{
			ServiceManagement.ServiceLocator<T>.SetService(allocatorId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveLocalService<T>(this ref AllocatorId allocatorId, T service)
		{
			ServiceManagement.ServiceLocator<T>.RemoveService(allocatorId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetLocalService<T>(this ref AllocatorId allocatorId)
		{
			return ServiceManagement.ServiceLocator<T>.GetService(allocatorId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetLocalService<T>(this ref AllocatorId allocatorId, out T service)
		{
			return ServiceManagement.ServiceLocator<T>.TryGetService(allocatorId, out service);
		}
	}
}
