using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>(WorldState worldState) where T : unmanaged, IWorldService
		{
			return GetOrRegisterServiceIndexedPtr<T>(worldState, out _);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>(WorldState worldState, out bool isExist) where T : unmanaged, IWorldService
		{
			EnsureInitialized(worldState);
			ref var slot = ref _services[worldState, TypeIdOf<IWorldService, T>.typeId];
			isExist = slot.IsCreated;
			if (!isExist)
				slot = new IndexedPtr(CachedPtr<T>.Create(worldState), TypeIdOf<T>.typeId);
			return slot;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(WorldState worldState) where T : unmanaged, IWorldService
		{
			return ref GetOrRegisterServiceIndexedPtr<T>(worldState).GetValue<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(WorldState worldState, out bool isExist) where T : unmanaged, IWorldService
		{
			return ref GetOrRegisterServiceIndexedPtr<T>(worldState, out isExist).GetValue<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetOrRegisterServiceCachedPtr<T>(WorldState worldState) where T : unmanaged, IWorldService
		{
			return GetOrRegisterServiceIndexedPtr<T>(worldState).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>(WorldState worldState) where T : unmanaged, IWorldService
		{
			return GetOrRegisterServiceIndexedPtr<T>(worldState).GetPtr<T>(worldState);
		}
	}
}
