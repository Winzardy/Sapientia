using System;
using System.Diagnostics;
using Sapientia.Extensions;

namespace Sapientia.Collections.Archetypes
{
	public abstract class EntityWorld : World
	{
		public int EntitiesCapacity { get; protected set; }

		protected override void AddStateParts()
		{
			AddStatePart(new EntitiesState(EntitiesCapacity));
		}
	}

	public abstract class World : IService
	{
		public static event Action OnGameStartEvent;
		public static event Action OnBeforeGameEndEvent;
		public static event Action OnGameEndEvent;
		public static event Action LateGameUpdateEvent;
		public static event Action LateGameUpdateOneShowEvent;

		public static World Instance => ServiceLocator<World>.Instance;

		public uint Tick { get; private set; }
		public float Time { get; private set; }

		public bool IsInitialized { get; private set; }
		public bool IsStarted { get; private set; }

		private readonly SimpleList<WorldElement> _elements = new ();
		private readonly SimpleList<Action> _unRegisterElementsActions = new ();
		private readonly SimpleList<WorldSystem> _systems = new ();

		private bool _stateInitialization;
		private bool _doLateGameUpdate;

		protected World()
		{
			Tick = 0;
			Time = 0f;

			IsInitialized = false;
			IsStarted = false;

			this.RegisterAsService();
		}

		public void Initialize()
		{
			_stateInitialization = true;
			AddStateParts();
			_stateInitialization = false;

			AddSystems();

			foreach (var element in _elements)
			{
				element.LateInitialize();
			}

			IsInitialized = true;
		}

		private void GameStart()
		{
			foreach (var element in _elements)
			{
				element.OnGameStart();
			}
			IsStarted = true;

			OnGameStartEvent?.Invoke();
		}

		public void Update(float deltaTime)
		{
			if (!IsStarted)
				GameStart();

			Tick++;
			Time += deltaTime;

			foreach (var system in _systems)
			{
				system.Update(deltaTime);
			}

			_doLateGameUpdate = true;
		}

		public void LateUpdate()
		{
			if (!_doLateGameUpdate)
				return;
			_doLateGameUpdate = false;

			LateGameUpdateEvent?.Invoke();
			LateGameUpdateOneShowEvent?.Invoke();
			LateGameUpdateOneShowEvent = null;
		}

		public virtual void DeInitialize()
		{
			OnBeforeGameEndEvent?.Invoke();

			foreach (var action in _unRegisterElementsActions)
			{
				action?.Invoke();
			}

			OnGameEndEvent?.Invoke();
			this.UnRegisterAsService();
		}

		protected virtual void AddStateParts() {}

		protected virtual void AddSystems() {}

		public TStatePart AddStatePart<TStatePart>() where TStatePart : WorldStatePart, new()
		{
			return AddStatePart(new TStatePart());
		}

		public TStatePart AddStatePart<TStatePart>(TStatePart systemGroup) where TStatePart : WorldStatePart
		{
			Debug.Assert(_stateInitialization);
			var statePart = AddGameWorldElement(systemGroup);
			statePart.AddStateParts();

			return statePart;
		}

		public TSystemGroup AddSystemGroup<TSystemGroup>() where TSystemGroup : WorldSystemGroup, new()
		{
			return AddSystemGroup(new TSystemGroup());
		}

		public TSystemGroup AddSystemGroup<TSystemGroup>(TSystemGroup systemGroup) where TSystemGroup : WorldSystemGroup
		{
			Debug.Assert(!_stateInitialization);
			return AddGameWorldElement(systemGroup);
		}

		public TSystem AddSystem<TSystem>() where TSystem : WorldSystem, new()
		{
			return AddSystem(new TSystem());
		}

		public TSystem AddSystem<TSystem>(TSystem system) where TSystem : WorldSystem
		{
			Debug.Assert(!_stateInitialization);

			system = AddGameWorldElement(system);
			_systems.Add(system);
			return system;
		}

		private TElement AddGameWorldElement<TElement>(TElement element) where TElement : WorldElement
		{
			Debug.Assert(!IsInitialized);

			_elements.Add(element);

			_unRegisterElementsActions.Add<Action>(() =>
			{
				element.UnRegisterAsService();
				element.DeInitialize();
			});

			element.World = this;
			element.Initialize();
			element.RegisterAsService();

			return element;
		}
	}

	public abstract class WorldElement : IService
	{
		public World World { get; internal set; }

		public virtual void Initialize() {}

		public virtual void LateInitialize() {}

		public virtual void OnGameStart() {}

		public virtual void DeInitialize() {}

		protected TService GetService<TService>() where TService : IService
		{
			return ServiceLocator<TService>.Instance;
		}

		protected void GetService<TService>(out TService service) where TService : IService
		{
			service = ServiceLocator<TService>.Instance;
		}
	}

	public abstract class WorldStatePart : WorldElement
	{
		public virtual void AddStateParts() {}

		protected TStatePart AddStatePart<TStatePart>() where TStatePart : WorldStatePart, new()
		{
			return World.AddStatePart<TStatePart>();
		}

		protected TStatePart AddStatePart<TStatePart>(TStatePart systemGroup) where TStatePart : WorldStatePart
		{
			return World.AddStatePart(systemGroup);
		}
	}

	public abstract class WorldSystemGroup : WorldElement
	{
		protected TSystemGroup AddSystemGroup<TSystemGroup>() where TSystemGroup : WorldSystemGroup, new()
		{
			return World.AddSystemGroup<TSystemGroup>();
		}

		protected TSystemGroup AddSystemGroup<TSystemGroup>(TSystemGroup systemGroup) where TSystemGroup : WorldSystemGroup
		{
			return World.AddSystemGroup(systemGroup);
		}

		protected TSystem AddSystem<TSystem>() where TSystem : WorldSystem, new()
		{
			return World.AddSystem<TSystem>();
		}

		protected TSystem AddSystem<TSystem>(TSystem system) where TSystem : WorldSystem
		{
			return World.AddSystem(system);
		}
	}

	public abstract class WorldSystem : WorldElement
	{
		public virtual void Update(float deltaTime) {}
	}
}