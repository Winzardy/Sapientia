using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.State;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial class World : IDisposable
	{
		public bool ScheduleLateUpdate { get; private set; }

		public WorldState worldState;

		public bool IsStarted { get; private set; }

		public WorldId Id => worldState.WorldId;

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
			using var updateScope = worldState.GetUpdateScope();

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
				element.Initialize(worldState, worldState, element);
			}

			LocalStatePartService.Initialize(worldState);

			foreach (ref var element in elementsService.worldElements.GetEnumerable(worldState))
			{
				element.LateInitialize(worldState, worldState, element);
			}
		}

		public void Start()
		{
			E.ASSERT(!IsStarted);

			using var scope = worldState.GetWorldScope();
			using var updateScope = worldState.GetUpdateScope();

			ref var elementsService = ref worldState.GetService<WorldElementsService>();
			foreach (ref var element in elementsService.worldElements.GetEnumerable(worldState))
			{
				element.EarlyStart(worldState, worldState, element);
			}
			LocalStatePartService.EarlyStart(worldState);

			foreach (ref var element in elementsService.worldElements.GetEnumerable(worldState))
			{
				element.Start(worldState, worldState, element);
			}
			LocalStatePartService.Start(worldState);
			IsStarted = true;

			SendStartedMessage();
		}

		public void Update(float deltaTime)
		{
			E.ASSERT(IsStarted);

			using var scope = worldState.GetWorldScope();

			using var updateScope = worldState.GetUpdateScope();
			worldState.Tick++;
			worldState.Time += deltaTime;

			ref var elementsService = ref worldState.GetService<WorldElementsService>();
			foreach (ref var system in elementsService.worldSystems.GetEnumerable(worldState))
			{
				system.BeforeUpdate(worldState, worldState, system);
			}

			foreach (ref var system in elementsService.worldSystems.GetEnumerable(worldState))
			{
				system.Update(worldState, worldState, system, deltaTime);
			}

			foreach (ref var system in elementsService.worldSystems.GetEnumerable(worldState))
			{
				system.AfterUpdate(worldState, worldState, system);
			}

			ScheduleLateUpdate = true;
		}

		public void LateUpdate()
		{
			if (!ScheduleLateUpdate)
				return;
			ScheduleLateUpdate = false;

			using var scope = worldState.GetWorldScope();
			using var updateScope = worldState.GetUpdateScope();

			ref var elementsService = ref worldState.GetService<WorldElementsService>();
			foreach (ref var system in elementsService.worldSystems.GetEnumerable(worldState))
			{
				system.BeforeLateUpdate(worldState, worldState, system);
			}

			foreach (ref var system in elementsService.worldSystems.GetEnumerable(worldState))
			{
				system.LateUpdate(worldState, worldState, system);
			}

			foreach (ref var system in elementsService.worldSystems.GetEnumerable(worldState))
			{
				system.AfterLateUpdate(worldState, worldState, system);
			}

			SendLateUpdateMessage();
		}

		public void Dispose()
		{
			using var scope = worldState.GetWorldScope();
			using var updateScope = worldState.GetUpdateScope();

			LocalStatePartService.Dispose(worldState);
			SendBeginDisposeMessage();

			ref var elementsService = ref worldState.GetService<WorldElementsService>();
			foreach (ref var element in elementsService.worldElements.GetEnumerable(worldState))
			{
				element.Dispose(worldState, worldState, element);
			}

			SendDisposedMessage();

			worldState.RemoveWorld();
		}

		public static implicit operator WorldState(World world) => world.worldState;
	}

	public static class WorldExtensions
	{
		public static T GetService<T>(this World world)
			where T : class, IIndexedType
			=> world.worldState.GetService<T>();

		public static ref T Get<T>(this World world, ServiceType type = ServiceType.WorldState)
			where T : unmanaged, IIndexedType
			=> ref world.worldState.GetService<T>(type);

		public static SafePtr<T> GetPtr<T>(this World world, ServiceType type = ServiceType.WorldState)
			where T : unmanaged, IIndexedType
			=> world.worldState.GetServicePtr<T>(type);

		public static ref T GetOrCreate<T>(this World world, ServiceType type = ServiceType.WorldState)
			where T : unmanaged, IInitializableService
			=> ref world.worldState.GetOrCreateService<T>(type);
	}
}
