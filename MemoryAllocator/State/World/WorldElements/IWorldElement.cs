using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public interface IWorldElement : IInterfaceProxyType
	{
		public virtual void Initialize(WorldState worldState, IndexedPtr self) {}

		public virtual void LateInitialize(WorldState worldState, IndexedPtr self) {}

		/// <summary>
		/// Right before first world update and Start
		/// </summary>
		public virtual void EarlyStart(WorldState worldState, IndexedPtr self) {}
		/// <summary>
		/// Right before first world update
		/// </summary>
		public virtual void Start(WorldState worldState, IndexedPtr self) {}

		public virtual void Dispose(WorldState worldState, IndexedPtr self) {}
	}
}
