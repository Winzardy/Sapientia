using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe interface IKillSubscriber : IInterfaceProxyType
	{
		public void EntityKilled(Allocator* allocator, in Entity entity);
	}

	public struct KillCallback
	{
		public Entity target;
		public ProxyPtr<IKillSubscriberProxy> callback;
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

	public struct DestroyStatePart : IWorldStatePart
	{
		public unsafe void Initialize(Allocator* allocator, IndexedPtr self)
		{
			Archetype.RegisterArchetype<KillElement>(allocator, 512).SetDestroyHandler<KillElementDestroyHandler>();
			Archetype.RegisterArchetype<KillRequest>(allocator, 64);
			Archetype.RegisterArchetype<DelayKillRequest>(allocator, 64);
			Archetype.RegisterArchetype<DestroyRequest>(allocator, 64);
		}
	}

	public unsafe struct KillElementDestroyHandler : IElementDestroyHandler<KillElement>
	{
		public void EntityPtrArrayDestroyed(Allocator* allocator, ArchetypeElement<KillElement>** elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i]->value;
				value.children.Clear();
				value.parents.Clear();
				value.killCallbackHolders.Clear();

				if (!value.killCallbacks.IsCreated)
					continue;
				foreach (KillCallback* component in value.killCallbacks.GetPtrEnumerable(allocator))
				{
					component->callback.Dispose(allocator);
				}
				value.killCallbacks.Clear();
			}
		}

		public void EntityArrayDestroyed(Allocator* allocator, ArchetypeElement<KillElement>* elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i].value;
				value.children.Clear();
				value.parents.Clear();
				value.killCallbackHolders.Clear();
				foreach (KillCallback* component in value.killCallbacks.GetPtrEnumerable(allocator))
				{
					component->callback.Dispose(allocator);
				}
				value.killCallbacks.Clear();
			}
		}
	}
}
