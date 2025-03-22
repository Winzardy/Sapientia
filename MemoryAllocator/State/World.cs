using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe partial struct World : IIndexedType
	{
		public uint Tick { get; private set; }
		public float Time { get; private set; }
		public bool ScheduleLateUpdate { get; private set; }

		public AllocatorId allocatorId;

		public List<ProxyPtr<IWorldElementProxy>> worldElements;
		public List<ProxyPtr<IWorldSystemProxy>> worldSystems;

		public bool IsStarted { get; private set; }

		public static World* Create(Allocator* allocator, int elementsCapacity = 64)
		{
			var worldPtr = allocator->MemAlloc<World>(out var world);
			world->Tick = 0u;
			world->Time = 0f;
			world->ScheduleLateUpdate = false;
			world->IsStarted = false;

			world->allocatorId = allocator->allocatorId;
			world->worldElements = new (allocator, elementsCapacity);
			world->worldSystems = new (allocator, elementsCapacity);

			allocator->RegisterService<World>(worldPtr);

			return world;
		}

		public void Initialize(IEnumerable<ProxyPtr<IWorldStatePartProxy>> stateParts, IEnumerable<ProxyPtr<IWorldSystemProxy>> systems)
		{
			using var scope = allocatorId.GetAllocatorScope(out var allocator);

			foreach (var statePart in stateParts)
			{
				AddWorldElement(allocator, statePart.ToProxy<IWorldElementProxy>());
			}
			foreach (var system in systems)
			{
				AddWorldElement(allocator, system.ToProxy<IWorldElementProxy>());
				worldSystems.Add(allocator, system.ToProxy<IWorldSystemProxy>());
			}

			foreach (ProxyPtr<IWorldElementProxy>* element in worldElements.GetPtrEnumerable(allocator))
			{
				element->Initialize(allocator, allocator, element->indexedPtr);
			}

			LocalStatePartService.Initialize(allocator);

			foreach (ProxyPtr<IWorldElementProxy>* element in worldElements.GetPtrEnumerable(allocator))
			{
				element->LateInitialize(allocator, allocator, element->indexedPtr);
			}
		}

		private void AddWorldElement(Allocator* allocator, ProxyPtr<IWorldElementProxy> element)
		{
			worldElements.Add(allocator, element);
			allocator->RegisterService(element);
		}

		public void Start()
		{
			E.ASSERT(!IsStarted);

			using var scope = allocatorId.GetAllocatorScope(out var allocator);

			foreach (ProxyPtr<IWorldElementProxy>* element in worldElements.GetPtrEnumerable(allocator))
			{
				element->Start(allocator, allocator, element->indexedPtr);
			}
			IsStarted = true;

			SendStartedMessage();
		}

		public void Update(float deltaTime)
		{
			E.ASSERT(IsStarted);

			using var scope = allocatorId.GetAllocatorScope(out var allocator);

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

			using var scope = allocatorId.GetAllocatorScope(out var allocator);

			foreach (ProxyPtr<IWorldSystemProxy>* system in worldSystems.GetPtrEnumerable(allocator))
			{
				system->LateUpdate(allocator, allocator, system->indexedPtr);
			}

			// TODO: TASK-1379 SendLateUpdateMessage();
			// TODO: TASK-1379 SendLateUpdateOnceMessage();
		}

		public void Dispose()
		{
			using var scope = allocatorId.GetAllocatorScope(out var allocator);
			SendBeginDisposeMessage();

			foreach (ProxyPtr<IWorldElementProxy>* element in worldElements.GetPtrEnumerable(allocator))
			{
				element->Dispose(allocator, allocator, element->indexedPtr);
			}

			SendDisposedMessage();

			allocatorId.RemoveAllocator();
			this = default;
		}
	}

	public unsafe interface IWorldElement : IInterfaceProxyType
	{
		public virtual void Initialize(Allocator* allocator, IndexedPtr self) {}

		public virtual void LateInitialize(Allocator* allocator, IndexedPtr self) {}

		/// <summary>
		/// Right before first world update
		/// </summary>
		public virtual void Start(Allocator* allocator, IndexedPtr self) {}

		public virtual void Dispose(Allocator* allocator, IndexedPtr self) {}
	}

	public unsafe interface IWorldSystem : IWorldElement
	{
		public virtual void Update(Allocator* allocator, IndexedPtr self, float deltaTime) {}
		public virtual void LateUpdate(Allocator* allocator, IndexedPtr self) {}
	}

	public interface IWorldStatePart : IWorldElement
	{
	}
}
