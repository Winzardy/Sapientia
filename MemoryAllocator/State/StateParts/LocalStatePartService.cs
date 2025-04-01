using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.ServiceManagement;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe interface IWoldLocalStatePart
	{
		public void Initialize(SafePtr<Allocator> allocator){}

		public void Dispose(SafePtr<Allocator> allocator){}
	}

	public unsafe class LocalStatePartService
	{
		public readonly SimpleList<IWoldLocalStatePart> localStateParts = new();

		public static void AddStatePart(SafePtr<Allocator> allocator, IWoldLocalStatePart statePart)
		{
			var service = ServiceContext<AllocatorId>.GetOrCreateService<LocalStatePartService>(allocator.Value().allocatorId);
			if (service == null)
			{
				service = new LocalStatePartService();
				ServiceContext<AllocatorId>.SetService(service);
			}
			service.localStateParts.Add(statePart);
		}

		public static void Initialize(SafePtr<Allocator> allocator)
		{
			var service = ServiceContext<AllocatorId>.GetOrCreateService<LocalStatePartService>(allocator.Value().allocatorId);

			foreach (var statePart in service.localStateParts)
			{
				statePart.Initialize(allocator);
			}
		}

		public static void Dispose(SafePtr<Allocator> allocator)
		{
			if (!ServiceLocator<AllocatorId, LocalStatePartService>.TryRemoveService(allocator.Value().allocatorId, out var service))
				return;

			foreach (var statePart in service.localStateParts)
			{
				statePart.Dispose(allocator);
			}
			service.localStateParts.Dispose();
		}
	}
}
