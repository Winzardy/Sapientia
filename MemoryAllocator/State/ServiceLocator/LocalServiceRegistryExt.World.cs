using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class LocalServiceRegistryExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetLocalService<T>(this WorldState worldState, T service)
		{
			ServiceManagement.ServiceLocator<T>.SetService(worldState.WorldId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveLocalService<T>(this WorldState worldState, T service)
		{
			ServiceManagement.ServiceLocator<T>.RemoveService(worldState.WorldId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetLocalService<T>(this WorldState worldState)
		{
			return ServiceManagement.ServiceLocator<T>.GetService(worldState.WorldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetLocalService<T>(this WorldState worldState, out T service)
		{
			return ServiceManagement.ServiceLocator<T>.TryGetService(worldState.WorldId, out service);
		}
	}
}
