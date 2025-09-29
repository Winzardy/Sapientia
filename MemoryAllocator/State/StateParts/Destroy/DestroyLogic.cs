using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public struct DestroyLogic : IInitializableService
	{
		private WorldState _worldState;

		private SafePtr<EntityStatePart> _entityStatePart;
		private ComponentSetContext<DestroyElement> _destroyElementSet;
		private ComponentSetContext<KillElement> _killElementSet;
		private ComponentSetContext<DestroyRequest> _destroyRequestSet;
		private ComponentSetContext<KillRequest> _killRequestSet;
		private ComponentSetContext<DelayKillRequest> _delayKillRequestSet;

		private ComponentSetContext<AliveDuration> _aliveDurationSet;
		private ComponentSetContext<AliveTimeDebt> _aliveTimeDebtSet;

		void IInitializableService.Initialize(WorldState worldState)
		{
			_worldState = worldState;
			_entityStatePart = worldState.GetServicePtr<EntityStatePart>();
			_destroyElementSet = new ComponentSetContext<DestroyElement>(worldState);
			_killElementSet = new ComponentSetContext<KillElement>(worldState);
			_destroyRequestSet = new ComponentSetContext<DestroyRequest>(worldState);
			_killRequestSet = new ComponentSetContext<KillRequest>(worldState);
			_delayKillRequestSet = new ComponentSetContext<DelayKillRequest>(worldState);

			_aliveDurationSet = new ComponentSetContext<AliveDuration>(worldState);
			_aliveTimeDebtSet = new ComponentSetContext<AliveTimeDebt>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref AliveDuration GetAliveDuration(Entity entity)
		{
			return ref _aliveDurationSet.GetElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref AliveDuration TryGetAliveDuration(Entity entity, out bool isExist)
		{
			return ref _aliveDurationSet.TryGetElement(entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetTimeDebt(Entity entity)
		{
			return _aliveTimeDebtSet.ReadElement(entity).timeDebt.GetValue(_worldState.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetTimeDebt(Entity entity, out float timeDebt)
		{
			return _aliveTimeDebtSet.ReadElement(entity).timeDebt.TryGetValue(_worldState.Tick, out timeDebt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetTimeDebt(Entity entity, float timeDebt)
		{
			_aliveTimeDebtSet.GetElement(entity).timeDebt = new OneShotValue<float>(_worldState.Tick, timeDebt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RequestDestroy(Entity entity)
		{
			E.ASSERT(IsAlive(entity));

			_destroyRequestSet.GetElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasDestroyRequest(Entity entity)
		{
			return _destroyRequestSet.HasElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RequestKill(Entity entity)
		{
			E.ASSERT(IsAlive(entity));
			_killRequestSet.GetElement(entity);
		}

		public void RequestKill(Entity entity, float delay)
		{
			E.ASSERT(IsAlive(entity));
			ref var request = ref _delayKillRequestSet.GetElement(entity, out var isCreated);
			if (isCreated || request.delay > delay)
				request.delay = delay;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasKillRequest(Entity entity)
		{
			return _killRequestSet.HasElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasDestroyOrKillRequest(Entity entity)
		{
			return _killRequestSet.HasElement(entity) || _destroyRequestSet.HasElement(entity);
		}

		public void AddDestroyParent(Entity child, Entity parent)
		{
			E.ASSERT(IsAlive(child));
			E.ASSERT(_entityStatePart.Value().IsEntityExist(_worldState, parent));

			if (_destroyRequestSet.HasElement(parent))
			{
				RequestKill(child);
				return;
			}

			ref var childElement = ref _destroyElementSet.GetElement(child);
			ref var parentElement = ref _destroyElementSet.GetElement(parent);

			if (!childElement.parents.IsCreated)
				childElement.parents = new MemList<Entity>(_worldState);
			if (!childElement.children.IsCreated)
				childElement.children = new MemList<Entity>(_worldState);

			childElement.parents.Add(_worldState, parent);
			parentElement.children.Add(_worldState, child);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddDestroyChild(Entity parent, Entity child)
		{
			AddKillParent(child, parent);
		}

		public void AddDestroyChildren(Entity parent, MemListEnumerable<Entity> children)
		{
			E.ASSERT(IsAlive(parent));

			ref var parentElement = ref _destroyElementSet.GetElement(parent);

			if (!parentElement.children.IsCreated)
				parentElement.children = new MemList<Entity>(_worldState);
			parentElement.children.AddRange(_worldState, children);

			foreach (var child in children)
			{
				E.ASSERT(IsAlive(child));
				ref var childElement = ref _destroyElementSet.GetElement(child);
				if (!childElement.parents.IsCreated)
					childElement.parents = new MemList<Entity>(_worldState);
				childElement.parents.Add(_worldState, parent);
			}
		}

		public void AddKillParent(Entity child, Entity parent)
		{
			E.ASSERT(IsAlive(child));
			E.ASSERT(_entityStatePart.Value().IsEntityExist(_worldState, parent));

			if (_destroyRequestSet.HasElement(parent))
			{
				RequestKill(child);
				return;
			}

			ref var childElement = ref _killElementSet.GetElement(child);
			ref var parentElement = ref _killElementSet.GetElement(parent);

			if (!childElement.parents.IsCreated)
				childElement.parents = new MemList<Entity>(_worldState);
			if (!childElement.children.IsCreated)
				childElement.children = new MemList<Entity>(_worldState);

			childElement.parents.Add(_worldState, parent);
			parentElement.children.Add(_worldState, child);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddKillChild(Entity parent, Entity child)
		{
			AddKillParent(child, parent);
		}

		public void AddKillChildren(Entity parent, MemListEnumerable<Entity> children)
		{
			E.ASSERT(IsAlive(parent));

			ref var parentElement = ref _killElementSet.GetElement(parent);

			if (!parentElement.children.IsCreated)
				parentElement.children = new MemList<Entity>(_worldState);
			parentElement.children.AddRange(_worldState, children);

			foreach (var child in children)
			{
				E.ASSERT(IsAlive(child));
				ref var childElement = ref _killElementSet.GetElement(child);
				if (!childElement.parents.IsCreated)
					childElement.parents = new MemList<Entity>(_worldState);
				childElement.parents.Add(_worldState, parent);
			}
		}

		public void AddKillCallback<T>(Entity holder, Entity target, in T callback = default) where T: unmanaged, IKillSubscriber
		{
			ref var holderElement = ref _killElementSet.GetElement(holder);

			if (!holderElement.killCallbacks.IsCreated)
				holderElement.killCallbacks = new MemList<KillCallback>(_worldState);
			holderElement.killCallbacks.Add(_worldState, new KillCallback
			{
				callback = ProxyPtr<IKillSubscriberProxy>.Create(_worldState, callback),
				target = target,
			});

			ref var targetElement = ref _killElementSet.GetElement(target);
			if (!targetElement.killCallbackHolders.IsCreated)
				targetElement.killCallbackHolders = new MemList<Entity>(_worldState);
			targetElement.killCallbackHolders.Add(_worldState, holder);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAlive(Entity entity)
		{
			if (_destroyRequestSet.HasElement(entity))
				return false;
			if (_killRequestSet.HasElement(entity))
				return false;
			return _entityStatePart.Value().IsEntityExist(_worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsExist(Entity entity)
		{
			return _entityStatePart.Value().IsEntityExist(_worldState, entity);
		}
	}
}
