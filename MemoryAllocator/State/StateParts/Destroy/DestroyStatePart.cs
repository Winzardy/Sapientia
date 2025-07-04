using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe interface IKillSubscriber : IInterfaceProxyType
	{
		public void EntityKilled(WorldState worldState, in Entity entity);
	}

	public struct KillCallback
	{
		public Entity target;
		public ProxyPtr<IKillSubscriberProxy> callback;
	}

	public struct KillElement : IComponent
	{
		public MemList<Entity> children;
		public MemList<Entity> parents;

		public MemList<Entity> killCallbackHolders;
		public MemList<KillCallback> killCallbacks;
	}

	public struct KillRequest : IComponent {}

	public struct DelayKillRequest : IComponent
	{
		public float delay;
	}

	public struct DestroyRequest : IComponent {}

	public struct DestroyStatePart : IWorldStatePart
	{
		public void Initialize(WorldState worldState, IndexedPtr self)
		{
			Archetype.RegisterArchetype<KillElement>(worldState, 2048).SetDestroyHandler<KillElementDestroyHandler>(worldState);
			Archetype.RegisterArchetype<KillRequest>(worldState, 256);
			Archetype.RegisterArchetype<DelayKillRequest>(worldState, 64);
			Archetype.RegisterArchetype<DestroyRequest>(worldState, 256);
		}
	}

	public unsafe struct KillElementDestroyHandler : IElementDestroyHandler<KillElement>
	{
		public void EntityPtrArrayDestroyed(WorldState worldState, ArchetypeElement<KillElement>** elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i]->value;
				value.children.Clear();
				value.parents.Clear();
				value.killCallbackHolders.Clear();

				if (!value.killCallbacks.IsCreated)
					continue;
				foreach (ref var component in value.killCallbacks.GetEnumerable(worldState))
				{
					component.callback.Dispose(worldState);
				}
				value.killCallbacks.Clear();
			}
		}

		public void EntityArrayDestroyed(WorldState worldState, ArchetypeElement<KillElement>* elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i].value;
				value.children.Clear();
				value.parents.Clear();
				value.killCallbackHolders.Clear();
				foreach (ref var component in value.killCallbacks.GetEnumerable(worldState))
				{
					component.callback.Dispose(worldState);
				}
				value.killCallbacks.Clear();
			}
		}
	}
}
