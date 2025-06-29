namespace Sapientia.MemoryAllocator.State
{
	public struct DestroySystem : IWorldSystem
	{
		public void Update(WorldState worldState, IndexedPtr self, float deltaTime)
		{
			var updater = new DestroyUpdater(worldState);
			updater.Update(deltaTime);
		}
	}
}
