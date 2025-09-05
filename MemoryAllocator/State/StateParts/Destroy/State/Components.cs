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

	public struct DestroyElement : IComponent
	{
		public MemList<Entity> children;
		public MemList<Entity> parents;
	}

	public struct KillRequest : IComponent {}

	public struct DelayKillRequest : IComponent
	{
		public float delay;
	}

	public struct DestroyRequest : IComponent {}

	public unsafe struct DestroyElementDestroyHandler : IElementDestroyHandler<DestroyElement>
	{
		public void EntityPtrArrayDestroyed(WorldState worldState, ComponentSetElement<DestroyElement>** elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i]->value;
				value.children.Dispose(worldState);
				value.parents.Dispose(worldState);
			}
		}

		public void EntityArrayDestroyed(WorldState worldState, ComponentSetElement<DestroyElement>* elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i].value;
				value.children.Dispose(worldState);
				value.parents.Dispose(worldState);
			}
		}
	}

	public unsafe struct KillElementDestroyHandler : IElementDestroyHandler<KillElement>
	{
		public void EntityPtrArrayDestroyed(WorldState worldState, ComponentSetElement<KillElement>** elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i]->value;
				value.children.Dispose(worldState);
				value.parents.Dispose(worldState);
				value.killCallbackHolders.Dispose(worldState);

				if (!value.killCallbacks.IsCreated)
					continue;
				foreach (ref var component in value.killCallbacks.GetEnumerable(worldState))
				{
					component.callback.Dispose(worldState);
				}
				value.killCallbacks.Dispose(worldState);
			}
		}

		public void EntityArrayDestroyed(WorldState worldState, ComponentSetElement<KillElement>* elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i].value;
				value.children.Dispose(worldState);
				value.parents.Dispose(worldState);
				value.killCallbackHolders.Dispose(worldState);

				if (!value.killCallbacks.IsCreated)
					continue;
				foreach (ref var component in value.killCallbacks.GetEnumerable(worldState))
				{
					component.callback.Dispose(worldState);
				}
				value.killCallbacks.Dispose(worldState);
			}
		}
	}
}
