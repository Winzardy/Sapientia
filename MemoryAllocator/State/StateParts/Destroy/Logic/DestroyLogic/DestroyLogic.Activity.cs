using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct DestroyLogic
	{
		public bool IsEnabled(Entity target)
		{
			return !_disabledSet.HasElement(target);
		}

		public void Enable(Entity target)
		{
			if (!_disabledSet.RemoveSwapBackElement(target))
				return;

			ref var targetComponent = ref _activityCallbackSet.TryGetElement(target, out var isExist);
			if (!isExist)
				return;

			foreach (ref var callback in targetComponent.enableCallbacks.GetEnumerable(_worldState))
			{
				callback.callback.OnEntityEnabled(_worldState, _worldState, target);
			}
		}

		public void Disable(Entity target)
		{
			_disabledSet.GetElement(target, out var isDisabled);
			if (isDisabled)
				return;

			ref var targetComponent = ref _activityCallbackSet.TryGetElement(target, out var isExist);
			if (!isExist)
				return;

			foreach (ref var callback in targetComponent.disableCallbacks.GetEnumerable(_worldState))
			{
				callback.callback.OnEntityDisabled(_worldState, _worldState, callback.callbackReceiver);
			}
		}

		public void AddEnableCallback<T>(Entity target, Entity callbackReceiver, in T callback = default) where T: unmanaged, IEnabledSubscriber
		{
			ref var targetComponent = ref _activityCallbackSet.GetElement(target);

			targetComponent.enableCallbacks.Add(_worldState, new Callback<IEnabledSubscriberProxy>
			{
				callback = ProxyPtr<IEnabledSubscriberProxy>.Create(_worldState, callback),
				callbackReceiver = callbackReceiver,
			});

			ref var receiverComponent = ref _activityCallbackSet.GetElement(callbackReceiver);
			receiverComponent.enableCallbackTargets.Add(_worldState, target);
		}

		public void AddDisableCallback<T>(Entity target, Entity callbackReceiver, in T callback = default) where T: unmanaged, IEnabledSubscriber
		{
			ref var targetComponent = ref _activityCallbackSet.GetElement(target);

			targetComponent.disableCallbacks.Add(_worldState, new Callback<IDisabledSubscriberProxy>
			{
				callback = ProxyPtr<IDisabledSubscriberProxy>.Create(_worldState, callback),
				callbackReceiver = callbackReceiver,
			});

			ref var receiverComponent = ref _activityCallbackSet.GetElement(callbackReceiver);
			receiverComponent.disableCallbackTargets.Add(_worldState, target);
		}
	}
}
