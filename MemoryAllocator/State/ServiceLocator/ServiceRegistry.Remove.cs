using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveService<T>(WorldState worldState) where T : unmanaged, IWorldService
		{
			if (!_services.IsCreated)
				return false;
			ref var slot = ref _services[worldState, TypeIdOf<IWorldService, T>.typeId];
			if (!slot.IsCreated)
				return false;
			slot = default;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveService<T>(WorldState worldState, out T service) where T : unmanaged, IWorldService
		{
			service = default;
			if (!_services.IsCreated)
				return false;
			ref var slot = ref _services[worldState, TypeIdOf<IWorldService, T>.typeId];
			if (!slot.IsCreated)
				return false;
			service = slot.GetValue<T>(worldState);
			slot = default;
			return true;
		}
	}
}
