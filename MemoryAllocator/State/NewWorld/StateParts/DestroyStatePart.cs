using System;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public unsafe struct KillElementDestroyHandler : IElementDestroyHandler<KillElement>
	{
		public void EntityDestroyed(ref ArchetypeElement<KillElement> element)
		{
			element.value.children.Clear();
			element.value.parents.Clear();
			element.value.killCallbackHolders.Clear();
			foreach (KillCallback* component in element.value.killCallbacks.GetIntPtrEnumerable())
			{
				component->callback.Dispose();
			}
			element.value.killCallbacks.Clear();
		}

		public void EntityArrayDestroyed(ArchetypeElement<KillElement>* element, uint count)
		{
			for (var i = 0u; i < count; i++)
			{
				element[i].value.children.Clear();
				element[i].value.parents.Clear();
				element[i].value.killCallbackHolders.Clear();
				foreach (KillCallback* component in element[i].value.killCallbacks.GetIntPtrEnumerable())
				{
					component->callback.Dispose();
				}
				element[i].value.killCallbacks.Clear();
			}
		}
	}

	[InterfaceProxy]
	public interface IKillSubscriber
	{
		public void EntityKilled(in Entity entity);
	}

	public struct KillCallback
	{
		public Entity target;
		public ProxyEvent<IKillSubscriberProxy> callback;
	}

	public struct KillElement : IComponent
	{
		public List<Entity> children;
		public List<Entity> parents;

		public List<Entity> killCallbackHolders;
		public List<KillCallback> killCallbacks;
	}

	public struct KillRequest : IComponent {}

	public struct DelayKillRequest : IComponent
	{
		public float delay;
	}

	public struct DestroyRequest : IComponent {}

	public unsafe struct DestroyStatePart : IWorldStatePart
	{
		private AllocatorId _allocatorId;
		public AllocatorId AllocatorId { get => _allocatorId; set => _allocatorId = value; }

		public Ptr<Archetype<KillElement>> killElementArchetypePtr;
		public Ptr<Archetype<KillRequest>> killRequestArchetypePtr;
		public Ptr<Archetype<DelayKillRequest>> delayKillRequestArchetypePtr;
		public Ptr<Archetype<DestroyRequest>> destroyRequestArchetypePtr;

		public void Initialize()
		{
			killElementArchetypePtr = Archetype<KillElement>.RegisterArchetype(AllocatorId, 512u);
			killRequestArchetypePtr = Archetype<KillRequest>.RegisterArchetype(AllocatorId, 64u);
			delayKillRequestArchetypePtr = Archetype<DelayKillRequest>.RegisterArchetype(AllocatorId, 64u);
			destroyRequestArchetypePtr = Archetype<DestroyRequest>.RegisterArchetype(AllocatorId, 64u);

			killElementArchetypePtr.GetValue().SetDestroyHandler<KillElementDestroyHandler>();
		}

		public void AdvanceTick(float deltaTime)
		{
			DestroyEntities();
			KillEntities(deltaTime);
		}

		private void DestroyEntities()
		{
			ref var destroyRequestArchetype = ref destroyRequestArchetypePtr.GetValue();

			var count = destroyRequestArchetype.Count;
			if (count < 1)
				return;

			ref var entityStatPart = ref _allocatorId.GetService<EntitiesStatePart>();

			var destroyRequests = destroyRequestArchetype.GetRawElements();
			var elementsToDestroy = stackalloc ArchetypeElement<DestroyRequest>[(int)count];

			MemoryExt.MemCopy(destroyRequests, elementsToDestroy, TSize<ArchetypeElement<DestroyRequest>>.uSize * count);
			destroyRequestArchetype.ClearFast();

			for (var i = 0; i < count; i++)
			{
				var entity = elementsToDestroy[i].entity;
				entityStatPart.DestroyEntity(entity);
			}
		}

		private void KillEntities(float deltaTime)
		{
			ref var delayKillRequestArchetype = ref delayKillRequestArchetypePtr.GetValue();
			var delayKillRequests = delayKillRequestArchetype.GetRawElements();

			for (var i = 0; i < delayKillRequestArchetype.Count; i++)
			{
				ref var request = ref delayKillRequests[i];
				if (request.value.delay <= 0)
					delayKillRequestArchetype.GetElement(request.entity);
				else
					request.value.delay -= deltaTime;
			}

			ref var destroyRequestArchetype = ref destroyRequestArchetypePtr.GetValue();
			ref var killRequestArchetype = ref killRequestArchetypePtr.GetValue();
			var killRequests = killRequestArchetype.GetRawElements();

			for (var i = 0; i < killRequestArchetype.Count; i++)
			{
				var entity = killRequests[i].entity;

				ExecuteKillCallback(entity);
				KillChild(entity);
				KillParent(entity);

				delayKillRequestArchetype.RemoveSwapBackElement(entity);
				destroyRequestArchetype.GetElement(entity);
			}

			killRequestArchetype.Clear();
		}

		private void ExecuteKillCallback(Entity entity)
		{
			if (!killElementsArchetype.HasElement(entity))
				return;
			ref var destroyElement = ref killElementsArchetype.GetElement(entity);

			if (destroyElement.killCallbacks != null)
			{
				for (var i = 0; i < destroyElement.killCallbacks.Count; i++)
				{
					ref var killCallback = ref destroyElement.killCallbacks[i];
					killCallback.callback?.Invoke(killCallback.target);
					killCallback.callback = null;

					if (!killElementsArchetype.HasElement(killCallback.target) || !killCallback.target.IsExist())
						continue;
					ref var targetElement = ref killElementsArchetype.GetElement(killCallback.target);
					if (targetElement.killCallbackHolders == null)
						continue;

					for (var j = 0; j < targetElement.killCallbackHolders.Count; j++)
					{
						if (targetElement.killCallbackHolders[i].id == entity.id)
							targetElement.killCallbackHolders.RemoveAtSwapBack(i);
					}
				}
				destroyElement.killCallbacks.ClearFast();
			}

			if (destroyElement.killCallbackHolders != null)
			{
				for (var i = 0; i < destroyElement.killCallbackHolders.Count; i++)
				{
					var holder = destroyElement.killCallbackHolders[i];
					if (!killElementsArchetype.HasElement(holder) || !holder.IsExist())
						continue;
					ref var holderElement = ref killElementsArchetype.GetElement(holder);
					for (var j = 0; j < holderElement.killCallbacks.Count; j++)
					{
						if (holderElement.killCallbacks[j].target.id == entity.id)
						{
							holderElement.killCallbacks[j].callback = null;
							holderElement.killCallbacks.RemoveAtSwapBack(j);
							break;
						}
					}
				}
				destroyElement.parents.ClearFast();
			}
		}

		private void KillChild(Entity entity)
		{
			if (!killElementsArchetype.HasElement(entity))
				return;
			ref var destroyElement = ref killElementsArchetype.GetElement(entity);

			if (destroyElement.parents == null)
				return;

			for (var i = 0; i < destroyElement.parents.Count; i++)
			{
				var parent = destroyElement.parents[i];
				if (!killElementsArchetype.HasElement(parent) || !parent.IsExist())
					continue;
				ref var parentElement = ref killElementsArchetype.GetElement(parent);
				for (var j = 0; j < parentElement.children.Count; j++)
				{
					if (parentElement.children[j].id == entity.id)
					{
						parentElement.children.RemoveAtSwapBack(j);
						break;
					}
				}
			}
			destroyElement.parents.ClearFast();
		}

		private void KillParent(Entity entity)
		{
			if (!killElementsArchetype.HasElement(entity))
				return;
			ref var destroyElement = ref killElementsArchetype.GetElement(entity);

			if (destroyElement.children == null)
				return;
			for (var i = 0; i < destroyElement.children.Count; i++)
			{
				var child = destroyElement.children[i];
				if (killRequestArchetypePtr.HasElement(child) || !child.IsExist())
					continue;
				killRequestArchetypePtr.GetElement(child);
				KillParent(child);
			}
			destroyElement.children.ClearFast();
		}
	}
}
