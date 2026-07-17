using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public struct WorldElementsService : IWorldService
	{
		public MemList<ProxyPtr<IWorldElementProxy>> worldElements;
		public MemList<ProxyPtr<IWorldSystemProxy>> worldSystems;
		public MemList<ProxyPtr<IWorldStatePartProxy>> worldStateParts;

		public WorldElementsService(WorldState worldState, int elementsCapacity = 64)
		{
			worldElements = new (worldState, elementsCapacity);
			worldSystems = new (worldState, elementsCapacity);
			worldStateParts = new (worldState, elementsCapacity);
		}

		private void AddWorldElement(WorldState worldState, ProxyPtr<IWorldElementProxy> element, TypeId<IWorldService> typeId)
		{
			worldElements.Add(worldState, element);
			worldState.RegisterService(typeId, (IndexedPtr)element);
		}

		public void AddWorldStatePart(WorldState worldState, ProxyPtr<IWorldStatePartProxy> statePart, TypeId<IWorldService> typeId)
		{
			AddWorldElement(worldState, statePart.ToProxy<IWorldElementProxy>(), typeId);
			worldStateParts.Add(worldState, statePart);
		}

		public void AddWorldSystem(WorldState worldState, ProxyPtr<IWorldSystemProxy> system, TypeId<IWorldService> typeId)
		{
			AddWorldElement(worldState, system.ToProxy<IWorldElementProxy>(), typeId);
			worldSystems.Add(worldState, system);
		}
	}
}
