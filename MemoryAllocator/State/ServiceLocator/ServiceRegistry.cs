using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		public MemArray<IndexedPtr> _services;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureInitialized(WorldState worldState)
		{
			if (!_services.IsCreated)
				_services = new MemArray<IndexedPtr>(worldState, TypeId<IWorldService>.Count);
		}
	}
}
