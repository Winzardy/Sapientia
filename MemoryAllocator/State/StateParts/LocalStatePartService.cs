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

	public interface IWorldLocalStatePart : IIndexedType
	{
		public void Initialize(WorldState worldState){}

		public void Dispose(WorldState worldState){}
	}

	public class LocalStatePartService
	{
		private readonly SimpleList<ClassPtr<IWorldLocalStatePart>> _localStateParts = new();
		private readonly SimpleList<UnsafeProxyPtr<IWorldUnmanagedLocalStatePartProxy>> _unmanagedLocalStateParts = new();

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
			service._unmanagedLocalStateParts.Add(proxyPtr);
		}

		public static void AddStatePart<T>(WorldState worldState, ClassPtr<T> statePartPtr)
			where T: class, IWorldLocalStatePart
		{
			var service = GetOrCreate(worldState);
			service._localStateParts.Add(ClassPtr<IWorldLocalStatePart>.Create(statePartPtr));
		}

		public static void Initialize(WorldState worldState)
		{
			var service = GetOrCreate(worldState);

			foreach (var statePart in service._localStateParts)
			{
				statePart.Value().Initialize(worldState);
			}

			foreach (var statePartPtr in service._unmanagedLocalStateParts)
			{
				statePartPtr.Initialize(worldState);
			}
		}

		public static void Dispose(WorldState worldState)
		{
			if (!ServiceLocator<WorldId, LocalStatePartService>.TryRemoveService(worldState.WorldId, out var service))
				return;

			foreach (var statePart in service._localStateParts)
			{
				statePart.Value().Dispose(worldState);
				statePart.Dispose();
			}
			service._localStateParts.Dispose();

			foreach (var statePartPtr in service._unmanagedLocalStateParts)
			{
				statePartPtr.Dispose(worldState);
			}
			service._unmanagedLocalStateParts.Dispose();
		}
	}
}
