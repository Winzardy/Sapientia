using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public struct WorldElementsService : IWorldService
	{
		public MemList<ProxyPtr<IWorldElementProxy>> worldElements;
		public MemList<ProxyPtr<IWorldSystemProxy>> worldSystems;

		public WorldElementsService(WorldState worldState, int elementsCapacity = 64)
		{
			worldElements = new (worldState, elementsCapacity);
			worldSystems = new (worldState, elementsCapacity);
		}

		public void AddWorldElement(WorldState worldState, ProxyPtr<IWorldElementProxy> element, TypeId<IWorldService> contextTypeId)
		{
			worldElements.Add(worldState, element);
			worldState.RegisterService(contextTypeId, (IndexedPtr)element);
		}

		public void AddWorldSystem(WorldState worldState, ProxyPtr<IWorldSystemProxy> system, TypeId<IWorldService> contextTypeId)
		{
			AddWorldElement(worldState, system.ToProxy<IWorldElementProxy>(), contextTypeId);
			worldSystems.Add(worldState, system);
		}
	}
}
