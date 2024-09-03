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

		public List<ProxyRef<IWorldElementProxy>> worldElements;
		public List<ProxyRef<IWorldSystemProxy>> worldSystems;

		public bool IsStarted => Tick > 0u;

		public static Ptr<World> Create(AllocatorId allocatorId, int elementsCount = 64)
		{
			var allocator = allocatorId.GetAllocatorPtr();

			var worldPtr = allocator->Alloc<World>(out var world);
			world->Tick = 0u;
			world->Time = 0f;
			world->AllowLateUpdate = false;

			world->allocatorId = allocatorId;
			world->worldElements = new (allocator, elementsCount);
			world->worldSystems = new (allocator, elementsCount);

			allocator->serviceLocator.RegisterService<World>(worldPtr);

			return worldPtr;
		}

		public void Initialize(IEnumerable<ProxyRef<IWorldStatePartProxy>> stateParts, IEnumerable<ProxyRef<IWorldSystemProxy>> systems)
		{
			using var scope = allocatorId.GetCurrentAllocatorScope();

			AddStateParts(stateParts);
			AddSystems(systems);

			foreach (ProxyRef<IWorldElementProxy>* element in worldElements.GetPtrEnumerable())
			{
				element->Initialize();
			}
			foreach (ProxyRef<IWorldElementProxy>* element in worldElements.GetPtrEnumerable())
			{
				element->LateInitialize();
			}
		}

		private void AddWorldElement(ProxyRef<IWorldElementProxy> element)
		{
			worldElements.Add(element);
			element.SetAllocatorId(allocatorId);

			allocatorId.RegisterService(element);
		}

		private void AddStateParts(IEnumerable<ProxyRef<IWorldStatePartProxy>> stateParts)
		{
			foreach (var statePart in stateParts)
			{
				AddWorldElement(statePart.ToProxy<IWorldElementProxy>());
				AddStateParts(statePart.GetStateParts());
			}
		}

		private void AddSystems(IEnumerable<ProxyRef<IWorldSystemProxy>> systems)
		{
			foreach (var system in systems)
			{
				AddWorldElement(system.ToProxy<IWorldElementProxy>());
				AddSystems(system.GetSystems());
			}
		}

		/// <summary>
		/// Right before first world update
		/// </summary>
		private void Start()
		{
			foreach (ProxyRef<IWorldElementProxy>* element in worldElements.GetPtrEnumerable())
			{
				element->Start();
			}

			SendStartedMessage();
		}

		public void Update(float deltaTime)
		{
			using var scope = allocatorId.GetCurrentAllocatorScope();

			if (!IsStarted)
				Start();

			Tick++;
			Time += deltaTime;

			foreach (ProxyRef<IWorldSystemProxy>* system in worldSystems.GetPtrEnumerable())
			{
				system->Update(deltaTime);
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
			SendBeginDisposeMessage();

			foreach (ProxyRef<IWorldElementProxy>* element in worldElements.GetPtrEnumerable())
			{
				element->Dispose();
			}

			SendDisposedMessage();

			allocatorId.RemoveAllocator();
			this = default;
		}
	}

	[InterfaceProxy]
	public interface IWorldElement : IIndexedType
	{
		public AllocatorId AllocatorId { get; set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetAllocatorId(AllocatorId allocatorId)
		{
			AllocatorId = allocatorId;
		}

		public virtual void Initialize() {}

		public virtual void LateInitialize() {}

		/// <summary>
		/// Right before first world update
		/// </summary>
		public virtual void Start() {}

		public virtual void Dispose() {}
	}

	public unsafe interface IWorldSystem : IWorldElement
	{
		public virtual IEnumerable<ProxyRef<IWorldSystemProxy>> GetSystems() => Array.Empty<ProxyRef<IWorldSystemProxy>>();
		public virtual void Update(float deltaTime) {}
	}

	public interface IWorldStatePart : IWorldElement
	{
		public virtual IEnumerable<ProxyRef<IWorldStatePartProxy>> GetStateParts() => Array.Empty<ProxyRef<IWorldStatePartProxy>>();
	}
}
