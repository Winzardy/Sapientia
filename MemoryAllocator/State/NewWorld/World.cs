using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public unsafe partial struct World : IIndexedType
	{
		public uint Tick { get; private set; }
		public float Time { get; private set; }
		public bool AllowLateUpdate { get; private set; }

		public AllocatorId allocatorId;

		public List<ProxyPtr<IWorldElementProxy>> worldElements;
		public List<ProxyPtr<IWorldSystemProxy>> worldSystems;

		public bool IsStarted => Tick > 0u;

		public static World* Create(Allocator* allocator, int elementsCount = 64)
		{
			var worldPtr = allocator->Alloc<World>(out var world);
			world->Tick = 0u;
			world->Time = 0f;
			world->AllowLateUpdate = false;

			world->allocatorId = allocator->allocatorId;
			world->worldElements = new (allocator, elementsCount);
			world->worldSystems = new (allocator, elementsCount);

			allocator->serviceLocator.RegisterService<World>(worldPtr);

			return world;
		}

		public void Initialize(IEnumerable<ProxyPtr<IWorldStatePartProxy>> stateParts, IEnumerable<ProxyPtr<IWorldSystemProxy>> systems)
		{
			using var scope = allocatorId.GetCurrentAllocatorScope(out var allocator);

			foreach (var statePart in stateParts)
			{
				AddWorldElement(allocator, statePart.ToProxy<IWorldElementProxy>());
			}
			foreach (var system in systems)
			{
				AddWorldElement(allocator, system.ToProxy<IWorldElementProxy>());
			}

			foreach (ProxyPtr<IWorldElementProxy>* element in worldElements.GetPtrEnumerable())
			{
				element->Initialize(allocator, allocator);
			}
			foreach (ProxyPtr<IWorldElementProxy>* element in worldElements.GetPtrEnumerable())
			{
				element->LateInitialize(allocator, allocator);
			}
		}

		private void AddWorldElement(Allocator* allocator, ProxyPtr<IWorldElementProxy> element)
		{
			worldElements.Add(allocator, element);
			allocator->RegisterService(element);
		}

		public void Update(float deltaTime)
		{
			using var scope = allocatorId.GetCurrentAllocatorScope(out var allocator);

			if (!IsStarted)
			{
				foreach (ProxyPtr<IWorldElementProxy>* element in worldElements.GetPtrEnumerable(allocator))
				{
					element->Start(allocator, allocator);
				}

				SendStartedMessage();
			}

			Tick++;
			Time += deltaTime;

			foreach (ProxyPtr<IWorldSystemProxy>* system in worldSystems.GetPtrEnumerable(allocator))
			{
				system->Update(allocator, allocator, deltaTime);
			}

			AllowLateUpdate = true;
		}

		public void LateUpdate()
		{
			if (!AllowLateUpdate)
				return;
			AllowLateUpdate = false;

			SendLateUpdateMessage();
			SendLateUpdateOnceMessage();
		}

		public void Dispose()
		{
			using var scope = allocatorId.GetCurrentAllocatorScope(out var allocator);
			SendBeginDisposeMessage();

			foreach (ProxyPtr<IWorldElementProxy>* element in worldElements.GetPtrEnumerable(allocator))
			{
				element->Dispose(allocator, allocator);
			}

			SendDisposedMessage();

			allocatorId.RemoveAllocator();
			this = default;
		}
	}

	[InterfaceProxy]
	public unsafe interface IWorldElement : IIndexedType
	{
		public virtual void Initialize(Allocator* allocator) {}

		public virtual void LateInitialize(Allocator* allocator) {}

		/// <summary>
		/// Right before first world update
		/// </summary>
		public virtual void Start(Allocator* allocator) {}

		public virtual void Dispose(Allocator* allocator) {}
	}

	public unsafe interface IWorldSystem : IWorldElement
	{
		public virtual void Update(Allocator* allocator, float deltaTime) {}
	}

	public interface IWorldStatePart : IWorldElement
	{
	}
}
