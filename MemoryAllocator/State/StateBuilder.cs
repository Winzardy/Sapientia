using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.ServiceManagement;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public abstract class EntityStateBuilder : StateBuilder
	{
		public int EntitiesCapacity { get; protected set; }

		public EntityStateBuilder(StateUpdateData stateUpdateData, int entitiesCapacity) : base(stateUpdateData)
		{
			EntitiesCapacity = entitiesCapacity;
		}

		protected override void AddStateParts()
		{
			base.AddStateParts();

			AddStatePart(new EntityStatePart(EntitiesCapacity));
			AddStatePart<DestroyStatePart>();
		}
	}

	public abstract class StateBuilder
	{
		protected World _world;

		private readonly SimpleList<ProxyPtr<IWorldStatePartProxy>> _stateParts = new();
		private readonly SimpleList<ProxyPtr<IWorldSystemProxy>> _systems = new();

		private readonly StateUpdateData _stateUpdateData;

		public StateBuilder(StateUpdateData stateUpdateData)
		{
			_stateUpdateData = stateUpdateData;
		}

		public State Build(int initialSize = -1)
		{
			_world = WorldManager.CreateWorld(initialSize);
			var world = WorldState.Create(_world);

			AddStateParts();
			AddSystems();

			InitializeWorld(world);

			return new State(_world.WorldId);
		}

		protected virtual void InitializeWorld(SafePtr<WorldState> worldState)
		{
			worldState.Value().Initialize(_stateParts, _systems);
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
			var ptr = _world.GetOrCreateUnmanagedLocalServicePtr<T>();
			ptr.Value() = value;

			LocalStatePartService.AddStatePart(_world, ptr);
		}

		public void AddLocalStatePart<T>() where T: IWorldLocalStatePart, new()
		{
			AddLocalStatePart(new T());
		}

		public void AddLocalStatePart<T>(in T value) where T: IWorldLocalStatePart
		{
			LocalStatePartService.AddStatePart(_world, value);
			ServiceContext<WorldId>.SetService(_world.WorldId, value);
		}

		public void AddStatePart<T>(in T value = default) where T: unmanaged, IWorldStatePart
		{
			var proxy = ProxyPtr<IWorldStatePartProxy>.Create(_world, value);
			_stateParts.Add(proxy);
		}

		public void AddStatePartGroup<T>() where T: WorldStatePartGroup, new()
		{
			var group = new T();
			group.AddStateParts(this);
		}

		public void AddSystem<T>() where T: unmanaged, IWorldSystem
		{
			var proxy = ProxyPtr<IWorldSystemProxy>.Create<T>(_world);
			_systems.Add(proxy);
		}

		public void AddSystemGroup<T>() where T: WorldSystemGroup, new()
		{
			var group = new T();
			group.AddSystems(this);
		}
	}

	public abstract class WorldStatePartGroup
	{
		private StateBuilder _builder;

		public void AddStateParts(StateBuilder builder)
		{
			_builder = builder;
			AddStateParts();
		}

		protected abstract void AddStateParts();

		protected void AddStatePartGroup<T>() where T: WorldSystemGroup, new()
		{
			_builder.AddSystemGroup<T>();
		}

		protected void AddStatePart<T>(in T value = default) where T: unmanaged, IWorldStatePart
		{
			_builder.AddStatePart<T>(value);
		}

		protected void AddUnmanagedLocalStatePart<T>() where T: unmanaged, IWorldUnmanagedLocalStatePart
		{
			_builder.AddUnmanagedLocalStatePart<T>();
		}

		protected void AddLocalStatePart<T>() where T: IWorldLocalStatePart, new()
		{
			_builder.AddLocalStatePart<T>();
		}

		protected void AddLocalStatePart<T>(in T value) where T: IWorldLocalStatePart
		{
			_builder.AddLocalStatePart<T>(value);
		}
	}

	public abstract class WorldSystemGroup
	{
		private StateBuilder _builder;

		public void AddSystems(StateBuilder builder)
		{
			_builder = builder;
			AddSystems();
		}

		protected abstract void AddSystems();

		protected void AddSystemGroup<T>() where T: WorldSystemGroup, new()
		{
			_builder.AddSystemGroup<T>();
		}

		protected void AddSystem<T>() where T: unmanaged, IWorldSystem
		{
			_builder.AddSystem<T>();
		}
	}
}
