using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public readonly unsafe ref struct DestroyLogic
	{
		public readonly World world;

		public readonly SafePtr<EntityStatePart> entityStatePart;
		public readonly ArchetypeContext<KillElement> killElementArchetype;
		public readonly ArchetypeContext<DestroyRequest> destroyRequestArchetype;
		public readonly ArchetypeContext<KillRequest> killRequestArchetype;
		public readonly ArchetypeContext<DelayKillRequest> delayKillRequestArchetype;

		public DestroyLogic(World world)
		{
			this.world = world;
			entityStatePart = world.GetServicePtr<EntityStatePart>();
			killElementArchetype = new ArchetypeContext<KillElement>(world);
			destroyRequestArchetype = new ArchetypeContext<DestroyRequest>(world);
			killRequestArchetype = new ArchetypeContext<KillRequest>(world);
			delayKillRequestArchetype = new ArchetypeContext<DelayKillRequest>(world);
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
			E.ASSERT(entityStatePart.ptr->IsEntityExist(world, parent));

			ref var childElement = ref killElementArchetype.GetElement(child);
			ref var parentElement = ref killElementArchetype.GetElement(parent);

			if (!childElement.parents.IsCreated)
				childElement.parents = new List<Entity>(world);
			if (!childElement.children.IsCreated)
				childElement.children = new List<Entity>(world);

			childElement.parents.Add(world, parent);
			parentElement.children.Add(world, child);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddKillChild(Entity parent, Entity child)
		{
			AddKillParent(child, parent);
		}

		public void AddKillChildren<T>(Entity parent, in T children) where T: IEnumerable<Entity>
		{
			E.ASSERT(IsAlive(parent));

			ref var parentElement = ref killElementArchetype.GetElement(parent);

			if (!parentElement.children.IsCreated)
				parentElement.children = new List<Entity>(world);
			parentElement.children.AddRange(world, children);

			foreach (var child in children)
			{
				E.ASSERT(IsAlive(child));
				ref var childElement = ref killElementArchetype.GetElement(child);
				if (!childElement.parents.IsCreated)
					childElement.parents = new List<Entity>(world);
				childElement.parents.Add(world, parent);
			}
		}

		public void AddKillCallback<T>(Entity holder, Entity target, in T callback = default) where T: unmanaged, IKillSubscriber
		{
			ref var holderElement = ref killElementArchetype.GetElement(holder);

			if (!holderElement.killCallbacks.IsCreated)
				holderElement.killCallbacks = new List<KillCallback>(world);
			holderElement.killCallbacks.Add(world, new KillCallback
			{
				callback = ProxyPtr<IKillSubscriberProxy>.Create(world, callback),
				target = target,
			});

			ref var targetElement = ref killElementArchetype.GetElement(target);
			if (!targetElement.killCallbackHolders.IsCreated)
				targetElement.killCallbackHolders = new List<Entity>(world);
			targetElement.killCallbackHolders.Add(world, holder);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAlive(Entity entity)
		{
			if (destroyRequestArchetype.HasElement(entity))
				return false;
			if (killRequestArchetype.HasElement(entity))
				return false;
			return entityStatePart.ptr->IsEntityExist(world, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsExist(Entity entity)
		{
			return entityStatePart.ptr->IsEntityExist(world, entity);
		}
	}
}
