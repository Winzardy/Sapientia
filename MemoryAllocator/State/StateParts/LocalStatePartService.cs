using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.MemoryAllocator;
using Sapientia.ServiceManagement;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public interface IWorldUnmanagedLocalStatePart : IInterfaceProxyType, IWorldLocalUnmanagedService
	{
		public void Initialize(WorldState worldState){}

		public void EarlyStart(WorldState worldState){}

		public void Start(WorldState worldState){}

		public void BeforeDispose(WorldState worldState){}

		public void Dispose(WorldState worldState){}
	}

	public interface IWorldLocalStatePart : IWorldLocalService
	{
		public void Initialize(WorldState worldState){}

		public void EarlyStart(WorldState worldState){}

		public void Start(WorldState worldState){}

		public void BeforeDispose(WorldState worldState){}

		public void Dispose(WorldState worldState){}
	}

	/// <summary>
	/// Координатор lifecycle для local state parts (managed + unmanaged) и общее хранилище
	/// managed-сервисов (через <see cref="IWorldLocalService"/>).
	///
	/// Managed-сервисы лежат в массиве <c>_managedServices</c>, проиндексированном по
	/// <c>TypeIdOf&lt;IWorldLocalService, T&gt;.typeId</c>. Unmanaged local state parts регистрируются
	/// в <see cref="UnsafeServiceRegistry"/> через <see cref="IWorldLocalUnmanagedService"/> маркер;
	/// здесь хранится только <see cref="UnsafeProxyPtr"/>-список для lifecycle iteration.
	/// </summary>
	public class LocalStatePartService
	{
		private ClassPtr[] _managedServices;
		private readonly SimpleList<UnsafeProxyPtr<IWorldUnmanagedLocalStatePartProxy>> _unmanagedLocalStateParts = new();
		/// <summary>
		/// Подмножество индексов в <see cref="_managedServices"/> для которых надо вызывать lifecycle
		/// (<see cref="IWorldLocalStatePart"/>-наследники, а не любой <see cref="IWorldLocalService"/>).
		/// Инвариант: <c>_managedLifecycleIndices ⊆ {i | _managedServices[i].IsValid}</c>.
		/// Поддерживается через assert в <see cref="AddStatePart{T}(WorldState, T)"/> и snapshot-clear в <see cref="Dispose"/>.
		/// </summary>
		private readonly SimpleList<int> _managedLifecycleIndices = new();

		private static LocalStatePartService GetOrCreate(WorldState worldState)
		{
			var service = ServiceContext<WorldId>.GetOrCreateService<LocalStatePartService>(worldState.WorldId);
			if (service == null)
			{
				service = new LocalStatePartService();
				ServiceContext<WorldId>.SetService(service);
			}

			return service;
		}

		private void EnsureManagedArray()
		{
			if (_managedServices != null)
				return;
			var count = TypeId<IWorldLocalService>.Count;
			_managedServices = count > 0 ? new ClassPtr[count] : System.Array.Empty<ClassPtr>();
		}

		public static void AddStatePart<T>(WorldState worldState, SafePtr<T> statePart) where T: unmanaged, IWorldUnmanagedLocalStatePart
		{
			var service = GetOrCreate(worldState);

			var proxyPtr = UnsafeProxyPtr<IWorldUnmanagedLocalStatePartProxy>.Create(statePart);
			service._unmanagedLocalStateParts.Add(proxyPtr);
		}

		public static void AddStatePart<T>(WorldState worldState, T statePart) where T : class, IWorldLocalStatePart
		{
			var service = GetOrCreate(worldState);
			service.EnsureManagedArray();

			var index = (int)TypeIdOf<IWorldLocalService, T>.typeId;
			ref var slot = ref service._managedServices[index];
			// Lifecycle index добавляется один раз: повторная регистрация одного T-state-парта
			// перезатёрла бы lifecycle hook двойным вызовом Initialize/Start/Dispose.
			E.ASSERT(!slot.IsValid, "AddStatePart вызван дважды для одного типа — это ошибка в WorldBuilder");
			slot = ClassPtr.Create(statePart);
			service._managedLifecycleIndices.Add(index);
		}

		internal static ref ClassPtr RegisterManagedService<T>(WorldState worldState, T service) where T : class, IWorldLocalService
		{
			var owner = GetOrCreate(worldState);
			owner.EnsureManagedArray();

			var index = (int)TypeIdOf<IWorldLocalService, T>.typeId;
			ref var slot = ref owner._managedServices[index];
			if (slot.IsValid)
				slot.Dispose();
			slot = ClassPtr.Create(service);
			return ref slot;
		}

		internal static bool TryGetManagedClassPtr<T>(WorldState worldState, out ClassPtr ptr) where T : class, IWorldLocalService
		{
			var owner = GetOrCreate(worldState);
			if (owner._managedServices == null)
			{
				ptr = default;
				return false;
			}
			var index = (int)TypeIdOf<IWorldLocalService, T>.typeId;
			ptr = owner._managedServices[index];
			return ptr.IsValid;
		}

		internal static T GetManagedService<T>(WorldState worldState) where T : class, IWorldLocalService
		{
			var owner = GetOrCreate(worldState);
			E.ASSERT(owner._managedServices != null);
			var index = (int)TypeIdOf<IWorldLocalService, T>.typeId;
			return owner._managedServices[index].Cast<T>();
		}

		internal static bool RemoveManagedService<T>(WorldState worldState) where T : class, IWorldLocalService
		{
			var owner = GetOrCreate(worldState);
			if (owner._managedServices == null)
				return false;
			var index = (int)TypeIdOf<IWorldLocalService, T>.typeId;
			ref var slot = ref owner._managedServices[index];
			if (!slot.IsValid)
				return false;
			slot.Dispose();
			slot = default;
			return true;
		}

		public static void Initialize(WorldState worldState)
		{
			var service = GetOrCreate(worldState);

			foreach (var index in service._managedLifecycleIndices)
			{
				service._managedServices[index].Cast<IWorldLocalStatePart>()?.Initialize(worldState);
			}

			foreach (var statePartPtr in service._unmanagedLocalStateParts)
			{
				statePartPtr.Initialize(worldState);
			}
		}

		public static void EarlyStart(WorldState worldState)
		{
			var service = GetOrCreate(worldState);

			foreach (var index in service._managedLifecycleIndices)
			{
				service._managedServices[index].Cast<IWorldLocalStatePart>()?.EarlyStart(worldState);
			}

			foreach (var statePartPtr in service._unmanagedLocalStateParts)
			{
				statePartPtr.EarlyStart(worldState);
			}
		}

		public static void Start(WorldState worldState)
		{
			var service = GetOrCreate(worldState);

			foreach (var index in service._managedLifecycleIndices)
			{
				service._managedServices[index].Cast<IWorldLocalStatePart>()?.Start(worldState);
			}

			foreach (var statePartPtr in service._unmanagedLocalStateParts)
			{
				statePartPtr.Start(worldState);
			}
		}

		public static void BeforeDispose(WorldState worldState)
		{
			var service = GetOrCreate(worldState);

			foreach (var index in service._managedLifecycleIndices)
			{
				service._managedServices[index].Cast<IWorldLocalStatePart>()?.BeforeDispose(worldState);
			}

			foreach (var statePartPtr in service._unmanagedLocalStateParts)
			{
				statePartPtr.BeforeDispose(worldState);
			}
		}

		public static void Dispose(WorldState worldState)
		{
			if (!ServiceLocator<WorldId, LocalStatePartService>.TryRemoveService(worldState.WorldId, out var service))
				return;

			foreach (var index in service._managedLifecycleIndices)
			{
				service._managedServices[index].Cast<IWorldLocalStatePart>()?.Dispose(worldState);
			}
			service._managedLifecycleIndices.Dispose();

			if (service._managedServices != null)
			{
				for (var i = 0; i < service._managedServices.Length; i++)
				{
					ref var slot = ref service._managedServices[i];
					if (slot.IsValid)
						slot.Dispose();
				}
				service._managedServices = null;
			}

			foreach (var statePartPtr in service._unmanagedLocalStateParts)
			{
				statePartPtr.Dispose(worldState);
			}
			service._unmanagedLocalStateParts.Dispose();
		}
	}
}
