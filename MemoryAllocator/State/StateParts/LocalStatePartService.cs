using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.ServiceManagement;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public interface IWorldUnmanagedLocalStatePart : IInterfaceProxyType
	{
		public void Initialize(WorldState worldState){}

		public void Dispose(WorldState worldState){}
	}

	public interface IWorldLocalStatePart
	{
		public void Initialize(WorldState worldState){}

		public void Dispose(WorldState worldState){}
	}

	public class LocalStatePartService
	{
		public readonly SimpleList<IWorldLocalStatePart> localStateParts = new();
		public readonly SimpleList<UnsafeProxyPtr<IWorldUnmanagedLocalStatePartProxy>> unmanagedLocalStateParts = new();

		private static LocalStatePartService GetOrCreate(WorldState worldState)
		{
			var service = ServiceContext<WorldId>.GetOrCreateService<LocalStatePartService>(worldState.WorldId);
			if (service == null)
			{
				service = new LocalStatePartService();
				ServiceContext<WorldId>.SetService(service);
			}

			return service;
		}

		public static void AddStatePart<T>(WorldState worldState, SafePtr<T> statePart) where T: unmanaged, IWorldUnmanagedLocalStatePart
		{
			var service = GetOrCreate(worldState);

			var proxyPtr = UnsafeProxyPtr<IWorldUnmanagedLocalStatePartProxy>.Create(statePart);
			service.unmanagedLocalStateParts.Add(proxyPtr);
		}

		public static void AddStatePart(WorldState worldState, IWorldLocalStatePart statePart)
		{
			var service = GetOrCreate(worldState);
			service.localStateParts.Add(statePart);
		}

		public static void Initialize(WorldState worldState)
		{
			var service = GetOrCreate(worldState);

			foreach (var statePart in service.localStateParts)
			{
				statePart.Initialize(worldState);
			}

			foreach (var statePartPtr in service.unmanagedLocalStateParts)
			{
				statePartPtr.Initialize(worldState);
			}
		}

		public static void Dispose(WorldState worldState)
		{
			if (!ServiceLocator<WorldId, LocalStatePartService>.TryRemoveService(worldState.WorldId, out var service))
				return;

			foreach (var statePart in service.localStateParts)
			{
				statePart.Dispose(worldState);
			}
			service.localStateParts.Dispose();

			foreach (var statePartPtr in service.unmanagedLocalStateParts)
			{
				statePartPtr.Dispose(worldState);
			}
			service.unmanagedLocalStateParts.Dispose();
		}
	}
}
