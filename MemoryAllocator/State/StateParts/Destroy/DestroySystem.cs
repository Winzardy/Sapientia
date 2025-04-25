using Sapientia.Data;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct DestroySystem : IWorldSystem
	{
		public void Update(Allocator allocator, IndexedPtr self, float deltaTime)
		{
			var updater = new DestroyUpdater(allocator);
			updater.Update(deltaTime);
		}
	}
}
