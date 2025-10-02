using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrCreateService<T>(WorldState worldState) where T: unmanaged, IInitializableService
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetOrCreateService<T>(worldState, typeIndex);
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrCreateService<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged, IInitializableService
		{
			ref var service = ref GetOrRegisterServiceIndexedPtr<T>(worldState, context, out var isExist).GetValue<T>(worldState);
			if (!isExist)
				service.Initialize(worldState);

			return ref service;
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreateServicePtr<T>(WorldState worldState) where T: unmanaged, IInitializableService
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetOrCreateServicePtr<T>(worldState, typeIndex);
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreateServicePtr<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged, IInitializableService
		{
			var servicePtr = GetOrRegisterServiceIndexedPtr<T>(worldState, context, out var isExist).GetPtr<T>(worldState);
			if (!isExist)
				servicePtr.Value().Initialize(worldState);

			return servicePtr;
		}
	}
}
