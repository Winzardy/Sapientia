using Sapientia.Collections;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public abstract class EntityStateBuilder : StateBuilder
	{
		public int EntitiesCapacity { get; protected set; }

		protected override void AddStateParts()
		{
			AddStatePart(new EntityStatePart(EntitiesCapacity));
		}
	}

	public unsafe class StateBuilder
	{
		protected Allocator* _allocator;

		private readonly SimpleList<ProxyPtr<IWorldStatePartProxy>> _stateParts = new();
		private readonly SimpleList<ProxyPtr<IWorldSystemProxy>> _systems = new();

		public AllocatorId Build(int initialSize = -1, int maxSize = -1)
		{
			AddStateParts();
			AddSystems();

			_allocator = AllocatorManager.CreateAllocator(initialSize, maxSize);
			var world = World.Create(_allocator);
			world->Initialize(_stateParts, _systems);

			return _allocator->allocatorId;
		}

		protected virtual void AddStateParts()
		{

		}

		protected virtual void AddSystems()
		{

		}

		public void AddLocalStatePart<T>() where T: IWoldLocalStatePart, new()
		{
			AddLocalStatePart(new T());
		}

		public void AddLocalStatePart<T>(in T value) where T: IWoldLocalStatePart
		{
			LocalStatePartService.AddStatePart(value);
		}

		public void AddStatePart<T>(in T value = default) where T: unmanaged, IWorldStatePart
		{
			var proxy = ProxyPtr<IWorldStatePartProxy>.Create(_allocator, value);
			_stateParts.Add(proxy);
		}

		public void AddStatePartGroup<T>() where T: WorldStatePartGroup, new()
		{
			var group = new T();
			group.AddStateParts(this);
		}

		public void AddSystem<T>() where T: unmanaged, IWorldSystem
		{
			var proxy = ProxyPtr<IWorldSystemProxy>.Create<T>(_allocator);
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

		protected void AddLocalStatePart<T>() where T: IWoldLocalStatePart, new()
		{
			_builder.AddLocalStatePart<T>();
		}

		protected void AddLocalStatePart<T>(in T value) where T: IWoldLocalStatePart
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
