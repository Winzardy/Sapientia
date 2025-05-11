using Sapientia.Data;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct DestroySystem : IWorldSystem
	{
		public void Update(World world, IndexedPtr self, float deltaTime)
		{
			var updater = new DestroyUpdater(world);
			updater.Update(deltaTime);
		}
	}
}
