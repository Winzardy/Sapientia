using Sapientia.Collections;
using Sapientia.MemoryAllocator;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct DestroyComponent : ICopiable<DestroyComponent>
	{
		public void AppendEntities(WorldState world, ref UnsafeList<Entity> entities)
		{
			// Владение kill-деревом уже задаёт KillCallbackComponent - здесь только зеркало связей, без Append.
		}

		public void InnerCopy(WorldState oldWS, WorldState newWS, ref DestroyComponent component, in EntityCopyMap map)
		{
			component.children = RemapAliveEntities(oldWS, newWS, children, map);
			component.parents = RemapAliveEntities(oldWS, newWS, parents, map);
		}

		private static MemList<Entity> RemapAliveEntities(WorldState oldWS, WorldState newWS, MemList<Entity> source, in EntityCopyMap map)
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
