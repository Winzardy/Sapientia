using Sapientia.Collections;
using Sapientia.MemoryAllocator;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct ActivityCallbackComponent : ICopiable<ActivityCallbackComponent>
	{
		public void AppendEntities(WorldState world, ref UnsafeList<Entity> entities)
		{
			// Payload подписки может владеть Entity-полями - отдаём их в обход.
			foreach (ref var callback in enableCallbacks.GetEnumerable(world))
			{
				callback.callback.AppendEntities(world, world, ref entities);
			}

			foreach (ref var callback in disableCallbacks.GetEnumerable(world))
			{
				callback.callback.AppendEntities(world, world, ref entities);
			}
		}

		public void InnerCopy(WorldState oldWS, WorldState newWS, ref ActivityCallbackComponent component, in EntityCopyMap map)
		{
			if (enableCallbackTargets.IsCreated)
			{
				component.enableCallbackTargets = new MemList<Entity>(newWS, enableCallbackTargets.Count);
				foreach (var target in enableCallbackTargets.GetEnumerable(oldWS))
				{
					var newTarget = map.GetOrDefault(target);
					if (newTarget.IsEmpty())
						continue;
					component.enableCallbackTargets.Add(newWS, newTarget);
				}
			}

			if (enableCallbacks.IsCreated)
			{
				component.enableCallbacks = new MemList<Callback<IEnabledSubscriberProxy>>(newWS, enableCallbacks.Count);
				foreach (ref var callback in enableCallbacks.GetEnumerable(oldWS))
				{
					var newReceiver = map.GetOrDefault(callback.callbackReceiver);
					if (newReceiver.IsEmpty())
						continue;

					component.enableCallbacks.Add(newWS, new Callback<IEnabledSubscriberProxy>
					{
						callbackReceiver = newReceiver,
						callback = callback.callback.Copy(oldWS, oldWS, newWS, map),
					});
				}
			}

			if (disableCallbackTargets.IsCreated)
			{
				component.disableCallbackTargets = new MemList<Entity>(newWS, disableCallbackTargets.Count);
				foreach (var target in disableCallbackTargets.GetEnumerable(oldWS))
				{
					var newTarget = map.GetOrDefault(target);
					if (newTarget.IsEmpty())
						continue;
					component.disableCallbackTargets.Add(newWS, newTarget);
				}
			}

			if (disableCallbacks.IsCreated)
			{
				component.disableCallbacks = new MemList<Callback<IDisabledSubscriberProxy>>(newWS, disableCallbacks.Count);
				foreach (ref var callback in disableCallbacks.GetEnumerable(oldWS))
				{
					var newReceiver = map.GetOrDefault(callback.callbackReceiver);
					if (newReceiver.IsEmpty())
						continue;

					component.disableCallbacks.Add(newWS, new Callback<IDisabledSubscriberProxy>
					{
						callbackReceiver = newReceiver,
						callback = callback.callback.Copy(oldWS, oldWS, newWS, map),
					});
				}
			}
		}
	}
}
