using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public interface IKillSubscriber : IInterfaceProxyType
	{
		public void EntityKilled(WorldState worldState, in Entity target);
	}

	public struct AliveDuration : IComponent
	{
		public float currentDuration;
		public OptionalValue<float> destroyDuration;
	}

	public struct AliveTimeDebt : IComponent
	{
		/// <summary>
		/// Время, которое нужно отнять от длительности жизни
		/// </summary>
		public OneShotValue<float> timeDebt;
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

	public struct KillRequest : IComponent
	{
		public bool dontDestroy;
	}

	public struct DelayKillRequest : IComponent
	{
		public float delay;
	}

	public struct DestroyRequest : IComponent {}

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
