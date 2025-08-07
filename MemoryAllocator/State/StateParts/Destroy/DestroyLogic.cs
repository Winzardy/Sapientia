using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public interface IDestroyLogicContainer
	{
		public DestroyLogic DestroyLogic { get; }
	}

	public readonly unsafe struct DestroyLogic : IDestroyLogicContainer
	{
		public readonly WorldState worldState;

		public readonly SafePtr<EntityStatePart> entityStatePart;
		public readonly ArchetypeContext<KillElement> killElementArchetype;
		public readonly ArchetypeContext<DestroyRequest> destroyRequestArchetype;
		public readonly ArchetypeContext<KillRequest> killRequestArchetype;
		public readonly ArchetypeContext<DelayKillRequest> delayKillRequestArchetype;

		public readonly ArchetypeContext<AliveDuration> aliveDurationContext;
		public readonly ArchetypeContext<AliveTimeDebt> aliveTimeDebtContext;

		public DestroyLogic(WorldState worldState)
		{
			this.worldState = worldState;
			entityStatePart = worldState.GetServicePtr<EntityStatePart>();
			killElementArchetype = new ArchetypeContext<KillElement>(worldState);
			destroyRequestArchetype = new ArchetypeContext<DestroyRequest>(worldState);
			killRequestArchetype = new ArchetypeContext<KillRequest>(worldState);
			delayKillRequestArchetype = new ArchetypeContext<DelayKillRequest>(worldState);

			aliveDurationContext = new ArchetypeContext<AliveDuration>(worldState);
			aliveTimeDebtContext = new ArchetypeContext<AliveTimeDebt>(worldState);
		}

		DestroyLogic IDestroyLogicContainer.DestroyLogic => this;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref AliveDuration GetAliveDuration(Entity entity)
		{
			return ref aliveDurationContext.GetElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref AliveDuration TryGetAliveDuration(Entity entity, out bool isExist)
		{
			return ref aliveDurationContext.TryGetElement(entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetTimeDebt(Entity entity)
		{
			return aliveTimeDebtContext.ReadElement(entity).timeDebt.GetValue(worldState.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetTimeDebt(Entity entity, out float timeDebt)
		{
			return aliveTimeDebtContext.ReadElement(entity).timeDebt.TryGetValue(worldState.Tick, out timeDebt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetTimeDebt(Entity entity, float timeDebt)
		{
			aliveTimeDebtContext.GetElement(entity).timeDebt = new OneShotValue<float>(worldState.Tick, timeDebt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RequestDestroy(Entity entity)
		{
			E.ASSERT(IsAlive(entity));

			destroyRequestArchetype.GetElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasDestroyRequest(Entity entity)
		{
			return destroyRequestArchetype.HasElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RequestKill(Entity entity)
		{
			E.ASSERT(IsAlive(entity));
			killRequestArchetype.GetElement(entity);
		}

		public void RequestKill(Entity entity, float delay)
		{
			E.ASSERT(IsAlive(entity));
			ref var request = ref delayKillRequestArchetype.GetElement(entity, out var isCreated);
			if (isCreated || request.delay > delay)
				request.delay = delay;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasKillRequest(Entity entity)
		{
			return killRequestArchetype.HasElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasDestroyOrKillRequest(Entity entity)
		{
			return killRequestArchetype.HasElement(entity) || destroyRequestArchetype.HasElement(entity);
		}

		public void AddKillParent(Entity child, Entity parent)
		{
			E.ASSERT(IsAlive(child));
			E.ASSERT(!destroyRequestArchetype.HasElement(parent));
			E.ASSERT(entityStatePart.ptr->IsEntityExist(worldState, parent));

			ref var childElement = ref killElementArchetype.GetElement(child);
			ref var parentElement = ref killElementArchetype.GetElement(parent);

			if (!childElement.parents.IsCreated)
				childElement.parents = new MemList<Entity>(worldState);
			if (!childElement.children.IsCreated)
				childElement.children = new MemList<Entity>(worldState);

			childElement.parents.Add(worldState, parent);
			parentElement.children.Add(worldState, child);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddKillChild(Entity parent, Entity child)
		{
			AddKillParent(child, parent);
		}

		public void AddKillChildren(Entity parent, MemListEnumerable<Entity> children)
		{
			E.ASSERT(IsAlive(parent));

			ref var parentElement = ref killElementArchetype.GetElement(parent);

			if (!parentElement.children.IsCreated)
				parentElement.children = new MemList<Entity>(worldState);
			parentElement.children.AddRange(worldState, children);

			foreach (var child in children)
			{
				E.ASSERT(IsAlive(child));
				ref var childElement = ref killElementArchetype.GetElement(child);
				if (!childElement.parents.IsCreated)
					childElement.parents = new MemList<Entity>(worldState);
				childElement.parents.Add(worldState, parent);
			}
		}

		public void AddKillCallback<T>(Entity holder, Entity target, in T callback = default) where T: unmanaged, IKillSubscriber
		{
			ref var holderElement = ref killElementArchetype.GetElement(holder);

			if (!holderElement.killCallbacks.IsCreated)
				holderElement.killCallbacks = new MemList<KillCallback>(worldState);
			holderElement.killCallbacks.Add(worldState, new KillCallback
			{
				callback = ProxyPtr<IKillSubscriberProxy>.Create(worldState, callback),
				target = target,
			});

			ref var targetElement = ref killElementArchetype.GetElement(target);
			if (!targetElement.killCallbackHolders.IsCreated)
				targetElement.killCallbackHolders = new MemList<Entity>(worldState);
			targetElement.killCallbackHolders.Add(worldState, holder);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAlive(Entity entity)
		{
			if (destroyRequestArchetype.HasElement(entity))
				return false;
			if (killRequestArchetype.HasElement(entity))
				return false;
			return entityStatePart.ptr->IsEntityExist(worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsExist(Entity entity)
		{
			return entityStatePart.ptr->IsEntityExist(worldState, entity);
		}
	}
}
