using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.ServiceManagement;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe interface IWoldLocalStatePart
	{
		public void Initialize(World world){}

		public void Dispose(World world){}
	}

	public unsafe class LocalStatePartService
	{
		public readonly SimpleList<IWoldLocalStatePart> localStateParts = new();

		public static void AddStatePart(World world, IWoldLocalStatePart statePart)
		{
			var service = ServiceContext<WorldId>.GetOrCreateService<LocalStatePartService>(world.worldId);
			if (service == null)
			{
				service = new LocalStatePartService();
				ServiceContext<WorldId>.SetService(service);
			}
			service.localStateParts.Add(statePart);
		}

		public static void Initialize(World world)
		{
			var service = ServiceContext<WorldId>.GetOrCreateService<LocalStatePartService>(world.worldId);

			foreach (var statePart in service.localStateParts)
			{
				statePart.Initialize(world);
			}
		}

		public static void Dispose(World world)
		{
			if (!ServiceLocator<WorldId, LocalStatePartService>.TryRemoveService(world.worldId, out var service))
				return;

			foreach (var statePart in service.localStateParts)
			{
				statePart.Dispose(world);
			}
			service.localStateParts.Dispose();
		}
	}
}
