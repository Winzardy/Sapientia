using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public interface IInitializableService : IIndexedType
	{
		public void Initialize(WorldState worldState);
	}
}
