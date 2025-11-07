using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public interface IKillSubscriber : IInterfaceProxyType
	{
		public void OnEntityKilled(WorldState worldState, in Entity callbackReceiver);
	}

	public struct KillCallbackComponent : IComponent
	{
		public MemList<Entity> children;
		public MemList<Entity> parents;

		/// <summary>
		/// Список сущностей, на которые подписана эта сущность.
		/// Нужно, чтобы при уничтожении этой сущности отписаться от колбэков.
		/// </summary>
		public MemList<Entity> callbackTargets;
		/// <summary>
		/// Список колбэков на уничтожение этой сущности
		/// </summary>
		public MemList<Callback<IKillSubscriberProxy>> killCallbacks;
	}

	public unsafe struct KillElementDestroyHandler : IElementDestroyHandler<KillCallbackComponent>
	{
		public void EntityPtrArrayDestroyed(WorldState worldState, ComponentSetElement<KillCallbackComponent>** elementsPtr, int count)
		{
			var componentSet = new ComponentSetContext<KillCallbackComponent>(worldState);
			for (var i = 0; i < count; i++)
			{
				ref var component = ref elementsPtr[i]->value;
				DisposeCallbacks(worldState, componentSet, ref component);
			}
		}

		public void EntityArrayDestroyed(WorldState worldState, ComponentSetElement<KillCallbackComponent>* elementsPtr, int count)
		{
			var componentSet = new ComponentSetContext<KillCallbackComponent>(worldState);
			for (var i = 0; i < count; i++)
			{
				ref var component = ref elementsPtr[i].value;
				DisposeCallbacks(worldState, componentSet, ref component);
			}
		}

		private static void DisposeCallbacks(WorldState worldState, ComponentSetContext<KillCallbackComponent> componentSet, ref KillCallbackComponent component)
		{
			component.children.Dispose(worldState);
			component.parents.Dispose(worldState);

			if (component.callbackTargets.IsCreated)
			{
				foreach (ref var target in component.callbackTargets.GetEnumerable(worldState))
				{
					ref var targetComponent = ref componentSet.TryGetElement(target, out var isExist);
					if (!isExist)
						continue;

					for (var i = 0; i < targetComponent.killCallbacks.Count; i++)
					{
						if (targetComponent.killCallbacks[worldState, i].callbackReceiver == target)
						{
							targetComponent.killCallbacks[worldState, i].callback.Dispose(worldState);
							targetComponent.killCallbacks.RemoveAtSwapBack(worldState, i);
							break;
						}
					}
				}
				component.callbackTargets.Dispose(worldState);
			}

			if (component.killCallbacks.IsCreated)
			{
				foreach (ref var callback in component.killCallbacks.GetEnumerable(worldState))
				{
					callback.callback.Dispose(worldState);
				}
				component.killCallbacks.Dispose(worldState);
			}
		}
	}
}
