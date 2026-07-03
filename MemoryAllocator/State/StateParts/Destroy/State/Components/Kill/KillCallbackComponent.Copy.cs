using Sapientia.Collections;
using Sapientia.MemoryAllocator;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct KillCallbackComponent : ICopiable<KillCallbackComponent>
	{
		public void AppendEntities(WorldState world, ref UnsafeList<Entity> entities)
		{
			if (!children.IsCreated)
			{
				return;
			}

			foreach (ref readonly var child in children.GetEnumerable(world))
			{
				// Протухшие ссылки бывают - уборка не чистит чужие списки при обычном киле.
				if (child.IsExist(world))
				{
					entities.Add(child);
				}
			}
		}

		public void InnerCopy(WorldState oldWS, WorldState newWS, ref KillCallbackComponent component, in UnsafeDictionary<Entity, Entity> map)
		{
			component.children = RemapAliveChildren(oldWS, newWS, children, map);
			component.parents = RemapAliveChildren(oldWS, newWS, parents, map);

			// callbackTargets/killCallbacks - ProxyPtr-подписки, локальны для мира, не копируются.
			component.callbackTargets = default;
			component.killCallbacks = default;
		}

		private static MemList<Entity> RemapAliveChildren(WorldState oldWS, WorldState newWS, MemList<Entity> source, in UnsafeDictionary<Entity, Entity> map)
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
