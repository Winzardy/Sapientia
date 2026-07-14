namespace Sapientia.MemoryAllocator.State
{
	public static class MemCopyExtensions
	{
		/// <summary>
		/// Пересоздаёт список ссылок в новом мире по карте пар: живая и переехавшая сущность -> её копия,
		/// протухшая или не переехавшая -> дроп. Для не созданного списка возвращает default.
		/// </summary>
		public static MemList<Entity> RemapAlive(this ref MemList<Entity> source, WorldState oldWS, WorldState newWS, in EntityCopyMap map)
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
