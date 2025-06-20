using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe interface IKillSubscriber : IInterfaceProxyType
	{
		public void EntityKilled(World world, in Entity entity);
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
		public unsafe void Initialize(World world, IndexedPtr self)
		{
			Archetype.RegisterArchetype<KillElement>(world, 2048).SetDestroyHandler<KillElementDestroyHandler>(world);
			Archetype.RegisterArchetype<KillRequest>(world, 256);
			Archetype.RegisterArchetype<DelayKillRequest>(world, 64);
			Archetype.RegisterArchetype<DestroyRequest>(world, 256);
		}
	}

	public unsafe struct KillElementDestroyHandler : IElementDestroyHandler<KillElement>
	{
		public void EntityPtrArrayDestroyed(World world, ArchetypeElement<KillElement>** elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i]->value;
				value.children.Clear();
				value.parents.Clear();
				value.killCallbackHolders.Clear();

				if (!value.killCallbacks.IsCreated)
					continue;
				foreach (KillCallback* component in value.killCallbacks.GetPtrEnumerable(world))
				{
					component->callback.Dispose(world);
				}
				value.killCallbacks.Clear();
			}
		}

		public void EntityArrayDestroyed(World world, ArchetypeElement<KillElement>* elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i].value;
				value.children.Clear();
				value.parents.Clear();
				value.killCallbackHolders.Clear();
				foreach (KillCallback* component in value.killCallbacks.GetPtrEnumerable(world))
				{
					component->callback.Dispose(world);
				}
				value.killCallbacks.Clear();
			}
		}
	}
}
