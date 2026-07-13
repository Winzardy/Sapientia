using Sapientia.Collections;
using Sapientia.MemoryAllocator;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct KillCallbackComponent : ICopiable<KillCallbackComponent>
	{
		public void AppendEntities(WorldState world, ref UnsafeList<Entity> entities)
		{
			// Kill-связь "слабая": children/parents в обход не тянем, до копии сущности добираются
			// явными owned-ссылками владельцев. InnerCopy оставит пары, где обе стороны перенеслись.
			// Payload подписки может владеть Entity-полями - отдаём их в обход.
			foreach (ref var callback in killCallbacks.GetEnumerable(world))
			{
				callback.callback.AppendEntities(world, world, ref entities);
			}
		}

		public void InnerCopy(WorldState oldWS, WorldState newWS, ref KillCallbackComponent component, in EntityCopyMap map)
		{
			component.children = children.RemapAlive(oldWS, newWS, in map);
			component.parents = parents.RemapAlive(oldWS, newWS, in map);

			// callbackTargets - обратный индекс "на кого я подписан", перенастраивается как обычный список ссылок.
			if (callbackTargets.IsCreated)
			{
				component.callbackTargets = new MemList<Entity>(newWS, callbackTargets.Count);
				foreach (var target in callbackTargets.GetEnumerable(oldWS))
				{
					var newTarget = map.GetOrDefault(target);
					if (newTarget.IsEmpty())
						continue;
					component.callbackTargets.Add(newWS, newTarget);
				}
			}

			// killCallbacks - ProxyPtr-подписки: payload копируется в новую арену через
			// ISubscriberCopyable.Copy (перенастраивает свои Entity-поля сам), callbackReceiver - обычное
			// поле-ссылка (map-or-EMPTY -> дроп).
			if (killCallbacks.IsCreated)
			{
				component.killCallbacks = new MemList<Callback<IKillSubscriberProxy>>(newWS, killCallbacks.Count);
				foreach (ref var callback in killCallbacks.GetEnumerable(oldWS))
				{
					var newReceiver = map.GetOrDefault(callback.callbackReceiver);
					if (newReceiver.IsEmpty())
						continue;

					component.killCallbacks.Add(newWS, new Callback<IKillSubscriberProxy>
					{
						callbackReceiver = newReceiver,
						callback = callback.callback.Copy(oldWS, oldWS, newWS, map),
					});
				}
			}
		}
	}
}
