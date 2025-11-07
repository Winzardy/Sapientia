using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public interface IEnabledSubscriber : IInterfaceProxyType
	{
		public void OnEntityEnabled(WorldState worldState, in Entity callbackReceiver);
	}

	public interface IDisabledSubscriber : IInterfaceProxyType
	{
		public void OnEntityDisabled(WorldState worldState, in Entity callbackReceiver);
	}

	public struct ActivityCallbackComponent : IComponent
	{
		/// <summary>
		/// Список сущностей, на которые подписана эта сущность.
		/// Нужно, чтобы при уничтожении этой сущности отписаться от колбэков.
		/// </summary>
		public MemList<Entity> enableCallbackTargets;
		public MemList<Callback<IEnabledSubscriberProxy>> enableCallbacks;

		/// <summary>
		/// Список сущностей, на которые подписана эта сущность.
		/// Нужно, чтобы при уничтожении этой сущности отписаться от колбэков.
		/// </summary>
		public MemList<Entity> disableCallbackTargets;
		public MemList<Callback<IDisabledSubscriberProxy>> disableCallbacks;
	}

	public unsafe struct ActivityComponentDestroyHandler : IElementDestroyHandler<ActivityCallbackComponent>
	{
		public void EntityPtrArrayDestroyed(WorldState worldState, ComponentSetElement<ActivityCallbackComponent>** elementsPtr, int count)
		{
			var componentSet = new ComponentSetContext<ActivityCallbackComponent>(worldState);
			for (var i = 0; i < count; i++)
			{
				ref var component = ref elementsPtr[i]->value;
				DisposeCallbacks(worldState, componentSet, ref component);
			}
		}

		public void EntityArrayDestroyed(WorldState worldState, ComponentSetElement<ActivityCallbackComponent>* elementsPtr, int count)
		{
			var componentSet = new ComponentSetContext<ActivityCallbackComponent>(worldState);
			for (var i = 0; i < count; i++)
			{
				ref var component = ref elementsPtr[i].value;
				DisposeCallbacks(worldState, componentSet, ref component);
			}
		}

		private static void DisposeCallbacks(WorldState worldState, ComponentSetContext<ActivityCallbackComponent> componentSet, ref ActivityCallbackComponent component)
		{
			if (component.enableCallbackTargets.IsCreated)
			{
				foreach (ref var target in component.enableCallbackTargets.GetEnumerable(worldState))
				{
					ref var targetComponent = ref componentSet.TryGetElement(target, out var isExist);
					if (!isExist)
						continue;

					for (var i = 0; i < targetComponent.enableCallbackTargets.Count; i++)
					{
						if (targetComponent.enableCallbacks[worldState, i].callbackReceiver == target)
						{
							targetComponent.enableCallbacks[worldState, i].callback.Dispose(worldState);
							targetComponent.enableCallbacks.RemoveAtSwapBack(worldState, i);
							break;
						}
					}
				}
				component.enableCallbackTargets.Dispose(worldState);
			}

			if (component.disableCallbackTargets.IsCreated)
			{
				foreach (ref var target in component.disableCallbackTargets.GetEnumerable(worldState))
				{
					ref var targetComponent = ref componentSet.TryGetElement(target, out var isExist);
					if (!isExist)
						continue;

					for (var i = 0; i < targetComponent.disableCallbackTargets.Count; i++)
					{
						if (targetComponent.disableCallbacks[worldState, i].callbackReceiver == target)
						{
							targetComponent.disableCallbacks[worldState, i].callback.Dispose(worldState);
							targetComponent.disableCallbacks.RemoveAtSwapBack(worldState, i);
							break;
						}
					}
				}
				component.disableCallbackTargets.Dispose(worldState);
			}

			if (component.enableCallbacks.IsCreated)
			{
				foreach (ref var callback in component.enableCallbacks.GetEnumerable(worldState))
				{
					callback.callback.Dispose(worldState);
				}
				component.enableCallbacks.Dispose(worldState);
			}

			if (component.disableCallbacks.IsCreated)
			{
				foreach (ref var callback in component.disableCallbacks.GetEnumerable(worldState))
				{
					callback.callback.Dispose(worldState);
				}
				component.disableCallbacks.Dispose(worldState);
			}
		}
	}
}
