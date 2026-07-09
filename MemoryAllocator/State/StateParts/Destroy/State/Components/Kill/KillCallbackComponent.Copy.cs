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
			component.children = RemapAliveChildren(oldWS, newWS, children, map);
			component.parents = RemapAliveChildren(oldWS, newWS, parents, map);

			// callbackTargets - обратный индекс "на кого я подписан", ремап как обычный список ссылок.
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
			// ISubscriberCopyable.Copy (ремапит свои Entity-поля сам), callbackReceiver - обычное
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

		private static MemList<Entity> RemapAliveChildren(WorldState oldWS, WorldState newWS, MemList<Entity> source, in EntityCopyMap map)
		{
			if (!source.IsCreated)
			{
				return default;
			}

			var result = new MemList<Entity>(newWS, source.Count);
			foreach (ref readonly var entity in source.GetEnumerable(oldWS))
			{
				if (!entity.IsExist(oldWS))
				{
					continue;
				}

				var newEntity = map.GetOrDefault(entity);
				if (newEntity.IsEmpty())
				{
					continue;
				}
				result.Add(newWS, newEntity);
			}
			return result;
		}
	}
}
