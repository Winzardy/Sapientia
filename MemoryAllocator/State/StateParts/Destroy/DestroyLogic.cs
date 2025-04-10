using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public readonly unsafe ref struct DestroyLogic
	{
		public readonly SafePtr<Allocator> allocator;

		public readonly SafePtr<EntityStatePart> entityStatePart;
		public readonly ArchetypeContext<KillElement> killElementArchetype;
		public readonly ArchetypeContext<DestroyRequest> destroyRequestArchetype;
		public readonly ArchetypeContext<KillRequest> killRequestArchetype;

		public DestroyLogic(SafePtr<Allocator> allocator)
		{
			this.allocator = allocator;
			entityStatePart = allocator.GetServicePtr<EntityStatePart>();
			killElementArchetype = new ArchetypeContext<KillElement>(allocator);
			destroyRequestArchetype = new ArchetypeContext<DestroyRequest>(allocator);
			killRequestArchetype = new ArchetypeContext<KillRequest>(allocator);
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
			ref var request = ref entity.Get<DelayKillRequest>(allocator, out var hasRequest);
			if (!hasRequest || request.delay > delay)
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
			E.ASSERT(IsAlive(parent));

			ref var childElement = ref killElementArchetype.GetElement(child);
			ref var parentElement = ref killElementArchetype.GetElement(parent);

			if (!childElement.parents.IsCreated)
				childElement.parents = new List<Entity>(allocator);
			if (!childElement.children.IsCreated)
				childElement.children = new List<Entity>(allocator);

			childElement.parents.Add(allocator, parent);
			parentElement.children.Add(allocator, child);
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
				parentElement.children = new List<Entity>(allocator);
			parentElement.children.AddRange(allocator, children);

			foreach (var child in children)
			{
				E.ASSERT(IsAlive(child));
				ref var childElement = ref killElementArchetype.GetElement(child);
				if (!childElement.parents.IsCreated)
					childElement.parents = new List<Entity>(allocator);
				childElement.parents.Add(allocator, parent);
			}
		}

		public void AddKillCallback<T>(Entity holder, Entity target, in T callback = default) where T: unmanaged, IKillSubscriber
		{
			ref var holderElement = ref killElementArchetype.GetElement(holder);

			if (!holderElement.killCallbacks.IsCreated)
				holderElement.killCallbacks = new List<KillCallback>(allocator);
			holderElement.killCallbacks.Add(allocator, new KillCallback
			{
				callback = ProxyPtr<IKillSubscriberProxy>.Create(allocator, callback),
				target = target,
			});

			ref var targetElement = ref killElementArchetype.GetElement(target);
			if (!targetElement.killCallbackHolders.IsCreated)
				targetElement.killCallbackHolders = new List<Entity>(allocator);
			targetElement.killCallbackHolders.Add(allocator, holder);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAlive(Entity entity)
		{
			if (destroyRequestArchetype.HasElement(entity))
				return false;
			if (killRequestArchetype.HasElement(entity))
				return false;
			return entityStatePart.ptr->IsEntityExist(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsExist(Entity entity)
		{
			return entityStatePart.ptr->IsEntityExist(allocator, entity);
		}
	}
}
