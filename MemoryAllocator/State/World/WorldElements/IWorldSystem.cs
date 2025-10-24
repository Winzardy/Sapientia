namespace Sapientia.MemoryAllocator
{
	public interface IWorldSystem : IWorldElement
	{
		public virtual void BeforeUpdate(WorldState worldState, IndexedPtr self) {}
		public virtual void Update(WorldState worldState, IndexedPtr self, float deltaTime) {}
		public virtual void AfterUpdate(WorldState worldState, IndexedPtr self) {}

		public virtual void BeforeLateUpdate(WorldState worldState, IndexedPtr self) {}
		public virtual void LateUpdate(WorldState worldState, IndexedPtr self) {}
		public virtual void AfterLateUpdate(WorldState worldState, IndexedPtr self) {}
	}
}
