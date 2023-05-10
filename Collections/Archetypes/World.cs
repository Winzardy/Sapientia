using System;
using System.Diagnostics;
using Sapientia.Extensions;

namespace Sapientia.Collections.Archetypes
{
	public abstract class EntityWorld : World
	{
		public int EntitiesCapacity { get; protected set; }

		protected sealed override void AddState()
		{
			AddStatePart(new EntitiesState(EntitiesCapacity));
			AddStateParts();
		}

		protected virtual void AddStateParts() {}
	}

	public abstract class World : IService
	{
		public static event Action OnGameStartEvent;
		public static event Action OnGameEndEvent;
		public static event Action LateGameUpdateEvent;

		public static World Instance => ServiceLocator<World>.Instance;

		private readonly SimpleList<GameWorldElement> _elements = new ();
		private readonly SimpleList<Action> _unRegisterElementsActions = new ();
		private readonly SimpleList<GameSystem> _systems = new ();

		public uint Tick { get; private set; }
		public float Time { get; private set; }

		public bool IsInitialized { get; private set; }
		public bool IsStarted { get; private set; }

		private bool _stateInitialization;

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
			AddState();
			_stateInitialization = false;

			AddElements();

			for (var i = 0; i < _elements.Count; i++)
			{
				_elements[i].LateInitialize();
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

		public virtual void DeInitialize()
		{
			this.UnRegisterAsService();
			foreach (var action in _unRegisterElementsActions)
			{
				action?.Invoke();
			}
			OnGameEndEvent?.Invoke();
		}

		protected virtual void AddState() {}

		protected virtual void AddElements() {}

		public TStatePart AddStatePart<TStatePart>() where TStatePart : GameStatePart, new()
		{
			return AddStatePart(new TStatePart());
		}

		public TStatePart AddStatePart<TStatePart>(TStatePart feature) where TStatePart : GameStatePart
		{
			Debug.Assert(_stateInitialization);
			var statePart = AddGameWorldElement(feature);
			statePart.AddStateParts();

			return statePart;
		}

		public TFeature AddFeature<TFeature>() where TFeature : GameFeature, new()
		{
			return AddFeature(new TFeature());
		}

		public TFeature AddFeature<TFeature>(TFeature feature) where TFeature : GameFeature
		{
			Debug.Assert(!_stateInitialization);
			return AddGameWorldElement(feature);
		}

		public TSystem AddSystem<TSystem>() where TSystem : GameSystem, new()
		{
			return AddSystem(new TSystem());
		}

		public TSystem AddSystem<TSystem>(TSystem system) where TSystem : GameSystem
		{
			Debug.Assert(!_stateInitialization);

			system = AddGameWorldElement(system);
			_systems.Add(system);
			return system;
		}

		private TElement AddGameWorldElement<TElement>(TElement element) where TElement : GameWorldElement
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

		public void Update(float deltaTime)
		{
			if (!IsStarted)
				GameStart();

			Tick++;
			Time += deltaTime;

			for (var i = 0; i < _systems.Count; i++)
			{
				_systems[i].Update(deltaTime);
			}
		}

		public void LateUpdate()
		{
			LateGameUpdateEvent?.Invoke();
		}
	}

	public abstract class GameWorldElement : IService
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

	public abstract class GameStatePart : GameWorldElement
	{
		public virtual void AddStateParts() {}

		protected TStatePart AddStatePart<TStatePart>() where TStatePart : GameStatePart, new()
		{
			return World.AddStatePart<TStatePart>();
		}

		protected TStatePart AddStatePart<TStatePart>(TStatePart feature) where TStatePart : GameStatePart
		{
			return World.AddStatePart(feature);
		}
	}

	public abstract class GameFeature : GameWorldElement
	{
		protected TFeature AddFeature<TFeature>() where TFeature : GameFeature, new()
		{
			return World.AddFeature<TFeature>();
		}

		protected TFeature AddFeature<TFeature>(TFeature feature) where TFeature : GameFeature
		{
			return World.AddFeature(feature);
		}

		protected TSystem AddSystem<TSystem>() where TSystem : GameSystem, new()
		{
			return World.AddSystem<TSystem>();
		}

		protected TSystem AddSystem<TSystem>(TSystem system) where TSystem : GameSystem
		{
			return World.AddSystem(system);
		}
	}

	public abstract class GameSystem : GameWorldElement
	{
		public virtual void Update(float deltaTime) {}
	}
}