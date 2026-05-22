using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(WorldState worldState, MemPtr ptr) where T : unmanaged, IWorldService
		{
			EnsureInitialized(worldState);
			_services[worldState, TypeIdOf<IWorldService, T>.typeId] = new IndexedPtr(ptr, TypeIdOf<T>.typeId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(WorldState worldState, CachedPtr<T> ptr) where T : unmanaged, IWorldService
		{
			EnsureInitialized(worldState);
			_services[worldState, TypeIdOf<IWorldService, T>.typeId] = new IndexedPtr(ptr, TypeIdOf<T>.typeId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(WorldState worldState, CachedPtr ptr) where T : unmanaged, IWorldService
		{
			EnsureInitialized(worldState);
			_services[worldState, TypeIdOf<IWorldService, T>.typeId] = new IndexedPtr(ptr, TypeIdOf<T>.typeId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(WorldState worldState, IndexedPtr indexedPtr) where T : unmanaged, IWorldService
		{
			EnsureInitialized(worldState);
			_services[worldState, TypeIdOf<IWorldService, T>.typeId] = indexedPtr;
		}

		/// <summary>
		/// Регистрация по типу из <see cref="IndexedPtr.typeId"/>. Slow path —
		/// используется когда конкретный тип не известен compile-time (например proxy-based регистрация в <see cref="State.WorldElementsService"/>).
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(WorldState worldState, IndexedPtr indexedPtr)
		{
			EnsureInitialized(worldState);
			IndexedTypes.GetContextTypeIdByGlobalId(typeof(IWorldService), indexedPtr.typeId, out var contextTypeId);
			_services[worldState, contextTypeId] = indexedPtr;
		}
	}
}
