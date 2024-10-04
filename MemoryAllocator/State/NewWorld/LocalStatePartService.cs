using System.Diagnostics;
using Sapientia.Collections;
using Sapientia.ServiceManagement;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public unsafe interface IWoldLocalStatePart
	{
		public void Initialize(Allocator* allocator){}

		public void Dispose(Allocator* allocator){}
	}

	public unsafe class LocalStatePartService
	{
		public readonly SimpleList<IWoldLocalStatePart> localStateParts = new();

		public static void AddStatePart(IWoldLocalStatePart statePart)
		{
			var service = ServiceContext<AllocatorId>.GetService<LocalStatePartService>();
			if (service == null)
			{
				service = new LocalStatePartService();
				ServiceContext<AllocatorId>.SetService(service);
			}
			service.localStateParts.Add(statePart);
		}

		public static void Initialize(Allocator* allocator)
		{
			var service = ServiceContext<AllocatorId>.GetService<LocalStatePartService>();
			Debug.Assert(service != null);

			foreach (var statePart in service.localStateParts)
			{
				statePart.Initialize(allocator);
			}
		}

		public static void Dispose(Allocator* allocator)
		{
			var service = ServiceContext<AllocatorId>.GetService<LocalStatePartService>();
			Debug.Assert(service != null);

			foreach (var statePart in service.localStateParts)
			{
				statePart.Dispose(allocator);
			}
			service.localStateParts.Dispose();

			ServiceContext<AllocatorId>.SetService<LocalStatePartService>(null);
		}
	}
}
