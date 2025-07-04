using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.State;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial class World : IDisposable
	{
		public bool ScheduleLateUpdate { get; private set; }

		public WorldState worldState;

		public bool IsStarted { get; private set; }

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => worldState.IsValid;
		}

		public World(WorldState worldState, int elementsCapacity = 64)
		{
			this.worldState = worldState;

			ScheduleLateUpdate = false;
			IsStarted = false;

			worldState.GetOrRegisterService<WorldElementsService>() = new WorldElementsService(worldState, elementsCapacity);
		}

		public void Initialize(IEnumerable<ProxyPtr<IWorldStatePartProxy>> stateParts, IEnumerable<ProxyPtr<IWorldSystemProxy>> systems)
		{
			using var scope = worldState.GetWorldScope();

			ref var elementsService = ref worldState.GetService<WorldElementsService>();

			foreach (var statePart in stateParts)
			{
				elementsService.AddWorldElement(worldState, statePart.ToProxy<IWorldElementProxy>());
			}
			foreach (var system in systems)
			{
				elementsService.AddWorldSystem(worldState, system);
			}

			foreach (ref var element in elementsService.worldElements.GetEnumerable(worldState))
			{
				element.Initialize(worldState, worldState, element.indexedPtr);
			}

			LocalStatePartService.Initialize(worldState);

			foreach (ref var element in elementsService.worldElements.GetEnumerable(worldState))
			{
				element.LateInitialize(worldState, worldState, element.indexedPtr);
			}
		}

		public void Start()
		{
			E.ASSERT(!IsStarted);

			using var scope = worldState.GetWorldScope();

			ref var elementsService = ref worldState.GetService<WorldElementsService>();
			foreach (ref var element in elementsService.worldElements.GetEnumerable(worldState))
			{
				element.Start(worldState, worldState, element.indexedPtr);
			}
			IsStarted = true;

			SendStartedMessage();
		}

		public void Update(float deltaTime)
		{
			E.ASSERT(IsStarted);

			using var scope = worldState.GetWorldScope();

			worldState.Tick++;
			worldState.Time += deltaTime;

			ref var elementsService = ref worldState.GetService<WorldElementsService>();
			foreach (ref var system in elementsService.worldSystems.GetEnumerable(worldState))
			{
				system.Update(worldState, worldState, system.indexedPtr, deltaTime);
			}

			ScheduleLateUpdate = true;
		}

		public void LateUpdate()
		{
			if (!ScheduleLateUpdate)
				return;
			ScheduleLateUpdate = false;

			using var scope = worldState.GetWorldScope();

			ref var elementsService = ref worldState.GetService<WorldElementsService>();
			foreach (ref var system in elementsService.worldSystems.GetEnumerable(worldState))
			{
				system.LateUpdate(worldState, worldState, system.indexedPtr);
			}

			SendLateUpdateMessage();
		}

		public void Dispose()
		{
			using var scope = worldState.GetWorldScope();

			LocalStatePartService.Dispose(worldState);
			SendBeginDisposeMessage();

			ref var elementsService = ref worldState.GetService<WorldElementsService>();
			foreach (ref var element in elementsService.worldElements.GetEnumerable(worldState))
			{
				element.Dispose(worldState, worldState, element.indexedPtr);
			}

			SendDisposedMessage();

			worldState.RemoveWorld();
		}
	}
}
