using Sapientia.Collections;
using Sapientia.ServiceManagement;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe interface IWoldLocalStatePart
	{
		public void Initialize(Allocator* allocator){}

		public void Dispose(Allocator* allocator){}
	}

	public unsafe class LocalStatePartService
	{
		public readonly SimpleList<IWoldLocalStatePart> localStateParts = new();

		public static void AddStatePart(Allocator* allocator, IWoldLocalStatePart statePart)
		{
			var service = ServiceContext<AllocatorId>.GetOrCreateService<LocalStatePartService>(allocator->allocatorId);
			if (service == null)
			{
				service = new LocalStatePartService();
				ServiceContext<AllocatorId>.SetService(service);
			}
			service.localStateParts.Add(statePart);
		}

		public static void Initialize(Allocator* allocator)
		{
			var service = ServiceContext<AllocatorId>.GetOrCreateService<LocalStatePartService>(allocator->allocatorId);

			foreach (var statePart in service.localStateParts)
			{
				statePart.Initialize(allocator);
			}
		}

		public static void Dispose(Allocator* allocator)
		{
			if (!ServiceLocator<AllocatorId, LocalStatePartService>.TryRemoveService(allocator->allocatorId, out var service))
				return;

			foreach (var statePart in service.localStateParts)
			{
				statePart.Dispose(allocator);
			}
			service.localStateParts.Dispose();
		}
	}
}
