using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.State;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// Фасад над <see cref="ServiceRegistry"/> (in-state сервисы) для типов
	/// помеченных маркером <see cref="IWorldService"/>: <see cref="IWorldElement"/>-наследники
	/// (StatePart-ы, системы) и <see cref="IConfigurationRuntime"/>-конфиги.
	/// </summary>
	public static class WorldStateServiceExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this WorldState worldState) where T : unmanaged, IWorldService
			=> ref worldState.GetServiceRegistry().GetService<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T TryGetService<T>(this WorldState worldState, out bool isExist) where T : unmanaged, IWorldService
			=> ref worldState.GetServiceRegistry().TryGetService<T>(worldState, out isExist);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServicePtr<T>(this WorldState worldState) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().GetServicePtr<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetServicePtr<T>(this WorldState worldState, out SafePtr<T> ptr) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().TryGetServicePtr(worldState, out ptr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr GetServiceIndexedPtr<T>(this WorldState worldState) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().GetServiceIndexedPtr<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CachedPtr<T> GetServiceCachedPtr<T>(this WorldState worldState) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().GetServiceCachedPtr<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<T>(this WorldState worldState) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().HasService<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this WorldState worldState, MemPtr ptr) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().RegisterService<T>(worldState, ptr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this WorldState worldState, CachedPtr<T> ptr) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().RegisterService(worldState, ptr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this WorldState worldState, CachedPtr ptr) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().RegisterService<T>(worldState, ptr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this WorldState worldState, IndexedPtr indexedPtr) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().RegisterService<T>(worldState, indexedPtr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this WorldState worldState, IndexedPtr indexedPtr)
			=> worldState.GetServiceRegistry().RegisterService(worldState, indexedPtr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveService<T>(this WorldState worldState) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().RemoveService<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveService<T>(this WorldState worldState, out T service) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().RemoveService(worldState, out service);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrRegisterService<T>(this WorldState worldState) where T : unmanaged, IWorldService
			=> ref worldState.GetServiceRegistry().GetOrRegisterService<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrRegisterService<T>(this WorldState worldState, out bool isExist) where T : unmanaged, IWorldService
			=> ref worldState.GetServiceRegistry().GetOrRegisterService<T>(worldState, out isExist);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetOrRegisterServicePtr<T>(this WorldState worldState) where T : unmanaged, IWorldService
			=> worldState.GetServiceRegistry().GetOrRegisterServicePtr<T>(worldState);
	}

	/// <summary>
	/// Фасад над <see cref="UnsafeServiceRegistry"/> (runtime-only heap сервисы, не в снапшоте)
	/// для типов помеченных <see cref="IWorldLocalUnmanagedService"/>: Logic'и
	/// (<see cref="IInitializableService"/>), а также <see cref="IWorldUnmanagedLocalStatePart"/>.
	/// </summary>
	public static class WorldStateLocalUnmanagedServiceExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
			=> ref worldState.GetNoStateServiceRegistry().Get<T>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServicePtr<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
			=> worldState.GetNoStateServiceRegistry().GetPtr<T>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetServicePtr<T>(this WorldState worldState, out SafePtr<T> ptr) where T : unmanaged, IWorldLocalUnmanagedService
			=> worldState.GetNoStateServiceRegistry().TryGetPtr(out ptr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
			=> worldState.GetNoStateServiceRegistry().Has<T>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrCreateService<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService, IInitializableService
			=> ref worldState.GetNoStateServiceRegistry().GetOrCreate<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetOrCreateServicePtr<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService, IInitializableService
			=> worldState.GetNoStateServiceRegistry().GetOrCreatePtr<T>(worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrRegisterService<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
			=> ref worldState.GetNoStateServiceRegistry().GetOrCreate<T>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetOrRegisterServicePtr<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
			=> worldState.GetNoStateServiceRegistry().GetOrCreatePtr<T>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveService<T>(this WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService
			=> worldState.GetNoStateServiceRegistry().Remove<T>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveService<T>(this WorldState worldState, out T service) where T : unmanaged, IWorldLocalUnmanagedService
			=> worldState.GetNoStateServiceRegistry().Remove(out service);
	}

	/// <summary>
	/// Фасад над <see cref="LocalStatePartService"/> для managed-сервисов, помеченных
	/// <see cref="IWorldLocalService"/>: managed <see cref="IWorldLocalStatePart"/>,
	/// <see cref="Tags.TagsMapping"/> и им подобные.
	/// </summary>
	public static class WorldStateLocalServiceExt
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
}
