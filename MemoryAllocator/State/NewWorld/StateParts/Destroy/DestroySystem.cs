namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public unsafe struct DestroySystem : IWorldSystem
	{
		public void Update(Allocator* allocator, float deltaTime)
		{
			var updater = new DestroyUpdater(allocator);
			updater.Update(deltaTime);
		}
	}
}
