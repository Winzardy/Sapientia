using Sapientia.Collections;
using Sapientia.MemoryAllocator.State;
using Sapientia.ServiceManagement;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public abstract class WorldBuilder
	{
		protected World _world;

		private readonly SimpleList<ProxyPtr<IWorldStatePartProxy>> _stateParts = new();
		private readonly SimpleList<ProxyPtr<IWorldSystemProxy>> _systems = new();

		private readonly StateUpdateData _stateUpdateData;

		public WorldBuilder(StateUpdateData stateUpdateData)
		{
			_stateUpdateData = stateUpdateData;
		}

		public World Build(int initialSize = -1)
		{
			_world = WorldManager.CreateWorld(initialSize);

			AddStateParts();
			AddSystems();

			InitializeWorld();

			return _world;
		}

		protected virtual void InitializeWorld()
		{
			_world.Initialize(_stateParts, _systems);
		}

		protected virtual void AddStateParts()
		{
			AddUnmanagedLocalStatePart(new UpdateLocalStatePart(_stateUpdateData));
		}

		protected virtual void AddSystems()
		{

		}

		public void AddUnmanagedLocalStatePart<T>() where T: unmanaged, IWorldUnmanagedLocalStatePart
		{
			AddUnmanagedLocalStatePart(new T());
		}

		public void AddUnmanagedLocalStatePart<T>(in T value) where T: unmanaged, IWorldUnmanagedLocalStatePart
		{
			var ptr = _world.worldState.GetOrRegisterServicePtr<T>(ServiceType.NoState);
			ptr.Value() = value;

			LocalStatePartService.AddStatePart(_world.worldState, ptr);
		}

		public void AddLocalStatePart<T>() where T: class, IWorldLocalStatePart, new()
		{
			AddLocalStatePart(new T());
		}

		public void AddLocalStatePart<T>(in T value) where T: class, IWorldLocalStatePart
		{
			var servicePtr = _world.worldState.RegisterService<T>(value);
			LocalStatePartService.AddStatePart(_world.worldState, servicePtr);
			ServiceContext<WorldId>.SetService(_world.worldState.WorldId, value);
		}

		public void AddStatePart<T>(in T value = default) where T: unmanaged, IWorldStatePart
		{
			var proxy = ProxyPtr<IWorldStatePartProxy>.Create(_world.worldState, value);
			_stateParts.Add(proxy);
		}

		public void AddStatePartGroup<T>() where T: WorldStatePartGroup, new()
		{
			var group = new T();
			group.AddStateParts(this);
		}

		public void AddSystem<T>() where T: unmanaged, IWorldSystem
		{
			var proxy = ProxyPtr<IWorldSystemProxy>.Create<T>(_world.worldState);
			_systems.Add(proxy);
		}

		public void AddSystemGroup<T>() where T: WorldSystemGroup, new()
		{
			var group = new T();
			group.AddSystems(this);
		}
	}
}
