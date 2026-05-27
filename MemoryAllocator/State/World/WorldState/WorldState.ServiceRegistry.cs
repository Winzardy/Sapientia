using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.State;
using Sapientia.TypeIndexer;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// Фасад над heap <see cref="UnsafeIndexedRegistry{IWorldService, IndexedPtr}"/>:
	/// <see cref="IWorldElement"/>-наследники (StatePart-ы, системы), <see cref="IConfigurationRuntime"/>,
	/// <see cref="ISave"/>-структуры. Payload — <see cref="IndexedPtr"/> со ссылкой в allocator,
	/// поэтому регистр попадает в снапшот.
	/// </summary>
	public static class WorldStateServiceExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this WorldState worldState) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			E.ASSERT(slot.IsCreated);
			return ref slot.GetValue<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T TryGetService<T>(this WorldState worldState, out bool isExist) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			isExist = slot.IsCreated;
			if (isExist)
				return ref slot.GetValue<T>(worldState);
			return ref worldState.GetZeroRef<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServicePtr<T>(this WorldState worldState) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			E.ASSERT(slot.IsCreated);
			return slot.GetPtr<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetServicePtr<T>(this WorldState worldState, out SafePtr<T> ptr) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			if (slot.IsCreated)
			{
				ptr = slot.GetPtr<T>(worldState);
				return true;
			}
			ptr = default;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr GetServiceIndexedPtr<T>(this WorldState worldState) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			E.ASSERT(slot.IsCreated);
			return slot;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CachedPtr<T> GetServiceCachedPtr<T>(this WorldState worldState) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			E.ASSERT(slot.IsCreated);
			return slot.GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<T>(this WorldState worldState) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().Get<T>().IsCreated;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this WorldState worldState, MemPtr ptr) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().Set<T>(new IndexedPtr(ptr, TypeIdOf<T>.typeId));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this WorldState worldState, CachedPtr<T> ptr) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().Set<T>(new IndexedPtr(ptr, TypeIdOf<T>.typeId));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this WorldState worldState, CachedPtr ptr) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().Set<T>(new IndexedPtr(ptr, TypeIdOf<T>.typeId));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this WorldState worldState, IndexedPtr indexedPtr) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().Set<T>(indexedPtr);

		/// <summary>
		/// Регистрация с уже известным per-context <see cref="TypeId{IWorldService}"/>. Используется когда
		/// конкретный T-параметр недоступен compile-time, но caller хранит/вычисляет typeId сам
		/// (например, <see cref="State.WorldElementsService.AddWorldElement"/>).
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this WorldState worldState, TypeId<IWorldService> typeId, IndexedPtr indexedPtr)
			=> worldState.GetServiceRegistry().Set(typeId, indexedPtr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveService<T>(this WorldState worldState) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			if (!slot.IsCreated)
				return false;
			slot = default;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveService<T>(this WorldState worldState, out T service) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			if (!slot.IsCreated)
			{
				service = default;
				return false;
			}
			service = slot.GetValue<T>(worldState);
			slot = default;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrRegisterService<T>(this WorldState worldState) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			if (!slot.IsCreated)
				slot = new IndexedPtr(CachedPtr<T>.Create(worldState), TypeIdOf<T>.typeId);
			return ref slot.GetValue<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrRegisterService<T>(this WorldState worldState, out bool isExist) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			isExist = slot.IsCreated;
			if (!isExist)
				slot = new IndexedPtr(CachedPtr<T>.Create(worldState), TypeIdOf<T>.typeId);
			return ref slot.GetValue<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetOrRegisterServicePtr<T>(this WorldState worldState) where T : unmanaged, IWorldService
		{
			ref var slot = ref worldState.GetServiceRegistry().Get<T>();
			if (!slot.IsCreated)
				slot = new IndexedPtr(CachedPtr<T>.Create(worldState), TypeIdOf<T>.typeId);
			return slot.GetPtr<T>(worldState);
		}
	}

	/// <summary>
	/// Фасад над heap <see cref="UnsafeIndexedRegistry{IWorldLocalUnmanagedService, SafePtr}"/>:
	/// Logic-структуры (<see cref="IInitializableService"/>), <see cref="IWorldUnmanagedLocalStatePart"/>.
	/// </summary>
	public static class WorldStateLocalUnmanagedServiceExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
		{
			ref var slot = ref worldState.GetNoStateServiceRegistry().Get<T>();
			E.ASSERT(slot.IsValid);
			return ref slot.Value<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServicePtr<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
		{
			ref var slot = ref worldState.GetNoStateServiceRegistry().Get<T>();
			E.ASSERT(slot.IsValid);
			return slot;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetServicePtr<T>(this WorldState worldState, out SafePtr<T> ptr) where T : unmanaged, IWorldLocalUnmanagedService
		{
			ref var slot = ref worldState.GetNoStateServiceRegistry().Get<T>();
			if (slot.IsValid)
			{
				ptr = slot;
				return true;
			}
			ptr = default;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
			=> worldState.GetNoStateServiceRegistry().Get<T>().IsValid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrCreateService<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService, IInitializableService
		{
			ref var slot = ref worldState.GetNoStateServiceRegistry().Get<T>();
			if (!slot.IsValid)
			{
				slot = MemoryExt.MemAllocAndClear<T>();
				slot.Value<T>().Initialize(worldState);
			}
			return ref slot.Value<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetOrCreateServicePtr<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService, IInitializableService
		{
			ref var slot = ref worldState.GetNoStateServiceRegistry().Get<T>();
			if (!slot.IsValid)
			{
				slot = MemoryExt.MemAllocAndClear<T>();
				slot.Value<T>().Initialize(worldState);
			}
			return slot;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrRegisterService<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
		{
			ref var slot = ref worldState.GetNoStateServiceRegistry().Get<T>();
			if (!slot.IsValid)
				slot = MemoryExt.MemAllocAndClear<T>();
			return ref slot.Value<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetOrRegisterServicePtr<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
		{
			ref var slot = ref worldState.GetNoStateServiceRegistry().Get<T>();
			if (!slot.IsValid)
				slot = MemoryExt.MemAllocAndClear<T>();
			return slot;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveService<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
		{
			ref var slot = ref worldState.GetNoStateServiceRegistry().Get<T>();
			if (!slot.IsValid)
				return false;
			MemoryExt.MemFree(slot);
			slot = default;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveService<T>(this WorldState worldState, out T service) where T : unmanaged, IWorldLocalUnmanagedService
		{
			ref var slot = ref worldState.GetNoStateServiceRegistry().Get<T>();
			if (!slot.IsValid)
			{
				service = default;
				return false;
			}
			service = slot.Value<T>();
			MemoryExt.MemFree(slot);
			slot = default;
			return true;
		}
	}

	/// <summary>
	/// Фасад над managed <see cref="LocalStatePartService"/> для типов помеченных <see cref="IWorldLocalService"/>.
	/// </summary>
	public static class WorldStateLocalServiceExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ClassPtr<T> RegisterService<T>(this WorldState worldState, T service) where T : class, IWorldLocalService
		{
			ref var ptr = ref LocalStatePartService.RegisterManagedService(worldState, service);
			return (ClassPtr<T>)ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetService<T>(this WorldState worldState) where T : class, IWorldLocalService
			=> LocalStatePartService.GetManagedService<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetServiceClass<T>(this WorldState worldState) where T : class, IWorldLocalService
			=> LocalStatePartService.GetManagedService<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ClassPtr<T> GetServiceClassPtr<T>(this WorldState worldState) where T : class, IWorldLocalService
		{
			LocalStatePartService.TryGetManagedClassPtr<T>(worldState, out var ptr);
			return (ClassPtr<T>)ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<T>(this WorldState worldState) where T : class, IWorldLocalService
			=> LocalStatePartService.TryGetManagedClassPtr<T>(worldState, out _);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<T>(this WorldState worldState) where T : class, IWorldLocalService
			=> LocalStatePartService.RemoveManagedService<T>(worldState);
	}

	/// <summary>
	/// Фасад над <see cref="UnsafeIndexedRegistry{IComponent, CachedPtr{ComponentSet}}"/> для <see cref="ComponentSet"/>.
	/// </summary>
	public static class WorldStateComponentExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<ComponentSet> GetComponentSetPtr<T>(this WorldState worldState) where T : unmanaged, IComponent
		{
			ref var slot = ref worldState.GetComponentsManager().Get<T>();
			E.ASSERT(slot.IsValid());
			return slot.GetPtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref ComponentSet GetComponentSet<T>(this WorldState worldState) where T : unmanaged, IComponent
			=> ref worldState.GetComponentSetPtr<T>().Value();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterComponentSet<T>(this WorldState worldState, CachedPtr<ComponentSet> componentSetPtr) where T : unmanaged, IComponent
			=> worldState.GetComponentsManager().Set<T>(componentSetPtr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasComponentSet<T>(this WorldState worldState) where T : unmanaged, IComponent
			=> worldState.GetComponentsManager().Get<T>().IsValid();
	}
}
