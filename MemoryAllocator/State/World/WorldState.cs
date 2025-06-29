using System.Collections.Generic;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe partial struct WorldState : IIndexedType
	{
		public uint Tick { get; private set; }
		public float Time { get; private set; }
		public bool ScheduleLateUpdate { get; private set; }

		public WorldId worldId;

		public List<ProxyPtr<IWorldElementProxy>> worldElements;
		public List<ProxyPtr<IWorldSystemProxy>> worldSystems;

		public bool IsStarted { get; private set; }

		public static SafePtr<WorldState> Create(World world, int elementsCapacity = 64)
		{
			var worldPtr = world.MemAlloc<WorldState>(out var worldSafePtr);
			ref var worldState = ref worldSafePtr.Value();
			worldState.Tick = 0u;
			worldState.Time = 0f;
			worldState.ScheduleLateUpdate = false;
			worldState.IsStarted = false;

			worldState.worldId = world.worldId;
			worldState.worldElements = new (world, elementsCapacity);
			worldState.worldSystems = new (world, elementsCapacity);

			world.RegisterService<WorldState>(worldPtr);

			return worldSafePtr;
		}

		public void Initialize(IEnumerable<ProxyPtr<IWorldStatePartProxy>> stateParts, IEnumerable<ProxyPtr<IWorldSystemProxy>> systems)
		{
			using var scope = worldId.GetAllocatorScope(out var allocator);

			foreach (var statePart in stateParts)
			{
				AddWorldElement(allocator, statePart.ToProxy<IWorldElementProxy>());
			}
			foreach (var system in systems)
			{
				AddWorldElement(allocator, system.ToProxy<IWorldElementProxy>());
				worldSystems.Add(allocator, system.ToProxy<IWorldSystemProxy>());
			}

			foreach (var element in worldElements.GetPtrEnumerable(allocator))
			{
				element.ptr->Initialize(allocator, allocator, element.ptr->indexedPtr);
			}

			LocalStatePartService.Initialize(allocator);

			foreach (var element in worldElements.GetPtrEnumerable(allocator))
			{
				element.ptr->LateInitialize(allocator, allocator, element.ptr->indexedPtr);
			}
		}

		private void AddWorldElement(World world, ProxyPtr<IWorldElementProxy> element)
		{
			worldElements.Add(world, element);
			world.RegisterService(element);
		}

		public void Start()
		{
			E.ASSERT(!IsStarted);

			using var scope = worldId.GetAllocatorScope(out var allocator);

			foreach (var element in worldElements.GetPtrEnumerable(allocator))
			{
				element.ptr->Start(allocator, allocator, element.ptr->indexedPtr);
			}
			IsStarted = true;

			SendStartedMessage();
		}

		public void Update(float deltaTime)
		{
			E.ASSERT(IsStarted);

			using var scope = worldId.GetAllocatorScope(out var allocator);

			Tick++;
			Time += deltaTime;

			foreach (ProxyPtr<IWorldSystemProxy>* system in worldSystems.GetPtrEnumerable(allocator))
			{
				system->Update(allocator, allocator, system->indexedPtr, deltaTime);
			}

			ScheduleLateUpdate = true;
		}

		public void LateUpdate()
		{
			if (!ScheduleLateUpdate)
				return;
			ScheduleLateUpdate = false;

			using var scope = worldId.GetAllocatorScope(out var allocator);

			foreach (var system in worldSystems.GetPtrEnumerable(allocator))
			{
				system.ptr->LateUpdate(allocator, allocator, system.ptr->indexedPtr);
			}

			SendLateUpdateMessage();
		}

		public void Dispose()
		{
			using var scope = worldId.GetAllocatorScope(out var allocator);
			SendBeginDisposeMessage();

			foreach (ProxyPtr<IWorldElementProxy>* element in worldElements.GetPtrEnumerable(allocator))
			{
				element->Dispose(allocator, allocator, element->indexedPtr);
			}

			SendDisposedMessage();

			worldId.RemoveAllocator();
			this = default;
		}
	}

	public unsafe interface IWorldElement : IInterfaceProxyType
	{
		public virtual void Initialize(World world, IndexedPtr self) {}

		public virtual void LateInitialize(World world, IndexedPtr self) {}

		/// <summary>
		/// Right before first world update
		/// </summary>
		public virtual void Start(World world, IndexedPtr self) {}

		public virtual void Dispose(World world, IndexedPtr self) {}
	}

	public unsafe interface IWorldSystem : IWorldElement
	{
		public virtual void Update(World world, IndexedPtr self, float deltaTime) {}
		public virtual void LateUpdate(World world, IndexedPtr self) {}
	}

	public interface IWorldStatePart : IWorldElement
	{
	}
}
