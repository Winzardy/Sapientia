using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public struct WorldElementsService : IIndexedType
	{
		public MemList<ProxyPtr<IWorldElementProxy>> worldElements;
		public MemList<ProxyPtr<IWorldSystemProxy>> worldSystems;

		public WorldElementsService(WorldState worldState, int elementsCapacity = 64)
		{
			worldElements = new (worldState, elementsCapacity);
			worldSystems = new (worldState, elementsCapacity);
		}

		public void AddWorldElement(WorldState worldState, ProxyPtr<IWorldElementProxy> element)
		{
			worldElements.Add(worldState, element);
			worldState.RegisterService(element);
		}

		public void AddWorldSystem(WorldState worldState, ProxyPtr<IWorldSystemProxy> system)
		{
			AddWorldElement(worldState, system.ToProxy<IWorldElementProxy>());
			worldSystems.Add(worldState, system);
		}
	}
}
