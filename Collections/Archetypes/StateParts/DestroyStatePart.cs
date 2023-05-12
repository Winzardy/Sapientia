using System;
using System.Diagnostics;
using Sapientia.Extensions;

namespace Sapientia.Collections.Archetypes.StateParts
{
	public struct KillCallback
	{
		public Entity target;
		public Action<Entity> callback;
	}

	public struct KillElement
	{
		public SimpleList<Entity> children;
		public SimpleList<Entity> parents;

		public SimpleList<KillCallback> killCallbacks;
		public SimpleList<Entity> killCallbackHolders;
	}

	public struct KillRequest
	{
		public float delay;
	}

	public class DestroyStatePart : WorldStatePart
	{
		public readonly Archetype<KillElement> killElementsArchetype;
		public readonly Archetype killRequestArchetype;
		public readonly Archetype<KillRequest> delayKillRequestArchetype;
		public readonly Archetype destroyRequestArchetype;

		public DestroyStatePart()
		{
			killElementsArchetype = new (512, InitializeDestroyElement);
			killRequestArchetype = new (64);
			delayKillRequestArchetype = new (64);
			destroyRequestArchetype = new (64);
		}

		private static void InitializeDestroyElement(ref ArchetypeElement<KillElement> element)
		{
			element.value.children?.ClearFast();
			element.value.parents?.ClearFast();
			element.value.killCallbacks?.ClearFast();
			element.value.killCallbackHolders?.ClearFast();
		}

		public void AdvanceTick(float deltaTime)
		{
			DestroyEntities();
			KillEntities(deltaTime);
		}

		private void DestroyEntities()
		{
			var entitiesToDestroy = destroyRequestArchetype.Elements;
			var count = destroyRequestArchetype.Count;

			for (var i = 0; i < count; i++)
			{
				var entity = entitiesToDestroy[i].entity;
				entity.Destroy();
			}

			destroyRequestArchetype.Clear();
		}

		private void KillEntities(float deltaTime)
		{
			ref readonly var delayKillRequests = ref delayKillRequestArchetype.Elements;

			for (var i = 0; i < delayKillRequestArchetype.Count; i++)
			{
				ref var request = ref delayKillRequests[i];
				if (request.value.delay <= 0)
					killRequestArchetype.GetElement(request.entity);
				else
					request.value.delay -= deltaTime;
			}

			ref readonly var killRequests = ref killRequestArchetype.Elements;

			for (var i = 0; i < killRequestArchetype.Count; i++)
			{
				var entity = killRequests[i].entity;

				ExecuteKillCallback(entity);
				KillChild(entity);
				KillParent(entity);

				delayKillRequestArchetype.RemoveElement(entity);
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
				if (killRequestArchetype.HasElement(child) || !child.IsExist())
					continue;
				killRequestArchetype.GetElement(child);
				KillParent(child);
			}
			destroyElement.children.ClearFast();
		}
	}

	public static class DestroyStatePartExt
	{
		public static void RequestDestroy(this Entity entity)
		{
			Debug.Assert(entity.IsNotDeath());

			var service = ServiceLocator<DestroyStatePart>.Instance;
			service.destroyRequestArchetype.GetElement(entity);

			Debug.Assert(!service.killElementsArchetype.HasElement(entity));
		}

		public static bool HasDestroyRequest(this Entity entity)
		{
			var service = ServiceLocator<DestroyStatePart>.Instance;
			return service.destroyRequestArchetype.HasElement(entity);
		}

		public static void RequestKill(this Entity entity)
		{
			Debug.Assert(entity.IsNotDeath());
			var service = ServiceLocator<DestroyStatePart>.Instance;
			service.killRequestArchetype.GetElement(entity);
		}

		public static void RequestKill(this Entity entity, float delay)
		{
			Debug.Assert(entity.IsNotDeath());
			var service = ServiceLocator<DestroyStatePart>.Instance;
			if (service.killRequestArchetype.HasElement(entity))
				return;
			ref var request = ref service.delayKillRequestArchetype.GetElement(entity);
			if (request.delay > delay)
				request.delay = delay;
		}

		public static bool HasKillRequest(this Entity entity)
		{
			var service = ServiceLocator<DestroyStatePart>.Instance;
			return service.killRequestArchetype.HasElement(entity);
		}

		public static bool HasDestroyOrKillRequest(this Entity entity)
		{
			var service = ServiceLocator<DestroyStatePart>.Instance;
			return service.killRequestArchetype.HasElement(entity) || service.destroyRequestArchetype.HasElement(entity);
		}

		public static void AddKillParent(this Entity child, Entity parent)
		{
			Debug.Assert(child.IsNotDeath());
			Debug.Assert(parent.IsNotDeath());
			var service = ServiceLocator<DestroyStatePart>.Instance;

			ref var childElement = ref service.killElementsArchetype.GetElement(child);
			ref var parentElement = ref service.killElementsArchetype.GetElement(parent);

			childElement.parents ??= new SimpleList<Entity>();
			parentElement.children ??= new SimpleList<Entity>();

			childElement.parents.Add(parent);
			parentElement.children.Add(child);
		}

		public static void AddKillChild(this Entity parent, Entity child)
		{
			child.AddKillParent(parent);
		}

		public static void AddKillChildren(this Entity parent, SimpleList<Entity> children)
		{
			Debug.Assert(parent.IsNotDeath());
			var service = ServiceLocator<DestroyStatePart>.Instance;

			ref var parentElement = ref service.killElementsArchetype.GetElement(parent);

			parentElement.children ??= new SimpleList<Entity>();
			parentElement.children.AddRange(children);

			for (var i = 0; i < children.Count; i++)
			{
				var child = children[i];
				Debug.Assert(child.IsNotDeath());
				ref var childElement = ref service.killElementsArchetype.GetElement(child);
				childElement.parents ??= new SimpleList<Entity>();
				childElement.parents.Add(parent);
			}
		}

		public static void AddKillCallback(this Entity holder, Entity target, Action<Entity> callback)
		{
			var service = ServiceLocator<DestroyStatePart>.Instance;

			ref var holderElement = ref service.killElementsArchetype.GetElement(holder);

			holderElement.killCallbacks ??= new ();
			holderElement.killCallbacks.Add(new KillCallback
			{
				callback = callback,
				target = target,
			});

			ref var targetElement = ref service.killElementsArchetype.GetElement(target);
			targetElement.killCallbackHolders ??= new ();
			targetElement.killCallbackHolders.Add(holder);
		}

		public static bool IsNotDeath(this Entity entity)
		{
			var service = ServiceLocator<DestroyStatePart>.Instance;
			return !service.destroyRequestArchetype.HasElement(entity) &&
			       !service.killRequestArchetype.HasElement(entity) &&
			       entity.IsExist();
		}

		public static bool IsDeath(this Entity entity)
		{
			var service = ServiceLocator<DestroyStatePart>.Instance;
			return service.destroyRequestArchetype.HasElement(entity) ||
			       service.killRequestArchetype.HasElement(entity) ||
			       !entity.IsExist();
		}
	}
}