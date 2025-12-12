namespace Sapientia.MemoryAllocator.State
{
	public struct DestroySystem : IWorldSystem
	{
		public void Update(WorldState worldState, IndexedPtr self, float deltaTime)
		{
			ref var logic = ref worldState.GetOrCreateService<DestroyUpdateLogic>(ServiceType.NoState);
			logic.Update(deltaTime);
		}
	}
}
