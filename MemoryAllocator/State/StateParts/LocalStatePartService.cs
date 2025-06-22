using Sapientia.Collections;
using Sapientia.ServiceManagement;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public interface IWorldUnmanagedLocalStatePart : IInterfaceProxyType
	{
		public void Initialize(World world){}

		public void Dispose(World world){}
	}

	public interface IWorldLocalStatePart
	{
		public void Initialize(World world){}

		public void Dispose(World world){}
	}

	public class LocalStatePartService
	{
		public readonly SimpleList<IWorldLocalStatePart> localStateParts = new();
		public readonly SimpleList<UnsafeProxyPtr<IWorldUnmanagedLocalStatePartProxy>> unmanagedLocalStateParts = new();

		private static LocalStatePartService GetOrCreate(World world)
		{
			var service = ServiceContext<WorldId>.GetOrCreateService<LocalStatePartService>(world.WorldId);
			if (service == null)
			{
				service = new LocalStatePartService();
				ServiceContext<WorldId>.SetService(service);
			}

			return service;
		}

		public static void AddStatePart<T>(World world, SafePtr<T> statePart) where T: unmanaged, IWorldUnmanagedLocalStatePart
		{
			var service = GetOrCreate(world);

			var proxyPtr = UnsafeProxyPtr<IWorldUnmanagedLocalStatePartProxy>.Create(statePart);
			service.unmanagedLocalStateParts.Add(proxyPtr);
		}

		public static void AddStatePart(World world, IWorldLocalStatePart statePart)
		{
			var service = GetOrCreate(world);
			service.localStateParts.Add(statePart);
		}

		public static void Initialize(World world)
		{
			var service = GetOrCreate(world);

			foreach (var statePart in service.localStateParts)
			{
				statePart.Initialize(world);
			}

			foreach (var statePartPtr in service.unmanagedLocalStateParts)
			{
				statePartPtr.Initialize(world);
			}
		}

		public static void Dispose(World world)
		{
			if (!ServiceLocator<WorldId, LocalStatePartService>.TryRemoveService(world.WorldId, out var service))
				return;

			foreach (var statePart in service.localStateParts)
			{
				statePart.Dispose(world);
			}
			service.localStateParts.Dispose();

			foreach (var statePartPtr in service.unmanagedLocalStateParts)
			{
				statePartPtr.Dispose(world);
			}
			service.unmanagedLocalStateParts.Dispose();
		}
	}
}
