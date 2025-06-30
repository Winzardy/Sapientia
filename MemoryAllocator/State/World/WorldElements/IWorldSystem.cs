namespace Sapientia.MemoryAllocator
{
	public interface IWorldSystem : IWorldElement
	{
		public virtual void Update(WorldState worldState, IndexedPtr self, float deltaTime) {}
		public virtual void LateUpdate(WorldState worldState, IndexedPtr self) {}
	}
}
