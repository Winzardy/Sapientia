using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct DestroySystem : IWorldSystem
	{
		public void Update(SafePtr<Allocator> allocator, IndexedPtr self, float deltaTime)
		{
			var updater = new DestroyUpdater(allocator);
			updater.Update(deltaTime);
		}
	}
}
