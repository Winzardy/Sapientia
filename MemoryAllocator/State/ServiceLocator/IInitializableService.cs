namespace Sapientia.MemoryAllocator
{
	public interface IInitializableService : IWorldLocalUnmanagedService
	{
		public void Initialize(WorldState worldState);
	}
}
