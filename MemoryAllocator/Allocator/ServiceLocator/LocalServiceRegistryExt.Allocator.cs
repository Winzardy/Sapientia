using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class LocalServiceRegistryExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetLocalService<T>(this SafePtr<Allocator> allocator, T service)
		{
			ServiceManagement.ServiceLocator<T>.SetService(allocator.Value().allocatorId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveLocalService<T>(this SafePtr<Allocator> allocator, T service)
		{
			ServiceManagement.ServiceLocator<T>.RemoveService(allocator.Value().allocatorId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetLocalService<T>(this SafePtr<Allocator> allocator)
		{
			return ServiceManagement.ServiceLocator<T>.GetService(allocator.Value().allocatorId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetLocalService<T>(this SafePtr<Allocator> allocator, out T service)
		{
			return ServiceManagement.ServiceLocator<T>.TryGetService(allocator.Value().allocatorId, out service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetLocalService<T>(this ref Allocator allocator, T service)
		{
			ServiceManagement.ServiceLocator<T>.SetService(allocator.allocatorId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveLocalService<T>(this ref Allocator allocator, T service)
		{
			ServiceManagement.ServiceLocator<T>.RemoveService(allocator.allocatorId, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetLocalService<T>(this ref Allocator allocator)
		{
			return ServiceManagement.ServiceLocator<T>.GetService(allocator.allocatorId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetLocalService<T>(this ref Allocator allocator, out T service)
		{
			return ServiceManagement.ServiceLocator<T>.TryGetService(allocator.allocatorId, out service);
		}
	}
}
