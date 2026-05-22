using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(WorldState worldState) where T : unmanaged, IWorldService
		{
			E.ASSERT(_services.IsCreated);
			ref var ptr = ref _services[worldState, TypeIdOf<IWorldService, T>.typeId];
			E.ASSERT(ptr.IsCreated);
			return ref ptr.GetValue<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(WorldState worldState, out bool isExist) where T : unmanaged, IWorldService
		{
			if (!_services.IsCreated)
			{
				isExist = false;
				return ref worldState.GetZeroRef<T>();
			}
			ref var ptr = ref _services[worldState, TypeIdOf<IWorldService, T>.typeId];
			isExist = ptr.IsCreated;
			if (isExist)
				return ref ptr.GetValue<T>(worldState);
			return ref worldState.GetZeroRef<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(WorldState worldState) where T : unmanaged, IWorldService
		{
			E.ASSERT(_services.IsCreated);
			var ptr = _services[worldState, TypeIdOf<IWorldService, T>.typeId];
			E.ASSERT(ptr.IsCreated);
			return ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>(WorldState worldState) where T : unmanaged, IWorldService
		{
			E.ASSERT(_services.IsCreated);
			var ptr = _services[worldState, TypeIdOf<IWorldService, T>.typeId];
			E.ASSERT(ptr.IsCreated);
			return ptr.GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>(WorldState worldState) where T : unmanaged, IWorldService
		{
			E.ASSERT(_services.IsCreated);
			var ptr = _services[worldState, TypeIdOf<IWorldService, T>.typeId];
			E.ASSERT(ptr.IsCreated);
			return ptr.GetPtr<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetServicePtr<T>(WorldState worldState, out SafePtr<T> ptr) where T : unmanaged, IWorldService
		{
			ptr = default;
			if (!_services.IsCreated)
				return false;
			ref var indexedPtr = ref _services[worldState, TypeIdOf<IWorldService, T>.typeId];
			if (!indexedPtr.IsCreated)
				return false;
			ptr = indexedPtr.GetPtr<T>(worldState);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasService<T>(WorldState worldState) where T : unmanaged, IWorldService
		{
			if (!_services.IsCreated)
				return false;
			return _services[worldState, TypeIdOf<IWorldService, T>.typeId].IsCreated;
		}
	}
}
