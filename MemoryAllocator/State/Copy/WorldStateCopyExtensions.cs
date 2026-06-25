using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Копирует сущность вместе с её дочерним поддеревом в другой мир (один раз, при переходе между
	/// этапами). Три прохода: собрать поддерево; создать все копии и запомнить пары старая-новая;
	/// скопировать значения и перенастроить ссылки. Три, а не два, потому что для перенастройки ссылки
	/// новая сущность уже должна существовать, поэтому создание идёт до копирования.
	/// </summary>
	public static class WorldStateCopyExtensions
	{
		/// <summary>
		/// Копирует <paramref name="root"/> с дочерним поддеревом из srcWorld в dstWorld, возвращает новый
		/// корень. Условие: dstWorld уже построен (наборы компонентов зарегистрированы), а
		/// <paramref name="root"/> не помечен <see cref="IgnoreEntityCopy"/>.
		/// </summary>
		public static Entity CopyEntityTree(this WorldState srcWorld, Entity root, WorldState dstWorld)
		{
			// Набор для метки может быть не зарегистрирован; тогда метки нет ни на одной сущности.
			var hasIgnoreSet = srcWorld.HasComponentSet<IgnoreEntityCopy>();

			// Предусловие (до аллокаций): корень должен копироваться. Пустой или помеченный IgnoreEntityCopy
			// корень не попадёт в map, и map[root] упал бы KeyNotFound - проверяем явным E.ASSERT в DEBUG.
			E.ASSERT(!root.IsEmpty(), "CopyEntityTree: root пустой - копировать нечего, нарушено предусловие.");
			E.ASSERT(!hasIgnoreSet || !root.Has<IgnoreEntityCopy>(srcWorld), "CopyEntityTree: root помечен IgnoreEntityCopy - корень не скопируется, map[root] упадёт. Нарушено предусловие.");

			var toCopy = new UnsafeList<Entity>(default, 16);
			var visited = new UnsafeHashSet<Entity>(default, 16);
			var frontier = new UnsafeList<Entity>(default, 16);
			var typeIds = new List<TypeId>();
			var map = default(UnsafeDictionary<Entity, Entity>);

			try
			{
				frontier.Add(root);

				// Проход A: собрать поддерево (обход, без повторов через visited).
				while (frontier.count > 0)
				{
					var entity = frontier.RemoveLast();
					// Пустая ссылка (необязательная дочерняя сущность не задана) не копируется: иначе для неё
					// создалась бы лишняя сущность-копия, и перенастройка ссылок вернула бы её вместо Entity.EMPTY.
					if (entity.IsEmpty())
					{
						continue;
					}
					if (!visited.Add(entity))
					{
						continue;
					}
					if (hasIgnoreSet && entity.Has<IgnoreEntityCopy>(srcWorld))
					{
						continue;
					}

					toCopy.Add(entity);

					srcWorld.CollectComponentTypeIds(entity, typeIds);
					foreach (var typeId in typeIds)
					{
						if (GeneratedCopier.IsCopiable(typeId))
						{
							GeneratedCopier.AppendEntities(typeId, srcWorld, entity, ref frontier);
						}
					}
				}

				// Проход B: создать все копии, чтобы на проходе C ссылки уже было куда перенастраивать.
				map = new UnsafeDictionary<Entity, Entity>(default, toCopy.count);
				foreach (var oldEntity in toCopy)
				{
					var newEntity = CreateEntityCopy(srcWorld, dstWorld, oldEntity);
					map.Add(oldEntity, newEntity);
				}

				// Проход C: скопировать значения и перенастроить ссылки по таблице.
				foreach (var oldEntity in toCopy)
				{
					var newEntity = map[oldEntity];

					srcWorld.CollectComponentTypeIds(oldEntity, typeIds);
					foreach (var typeId in typeIds)
					{
						if (GeneratedCopier.IsCopiable(typeId))
						{
							GeneratedCopier.CopyComponent(typeId, srcWorld, dstWorld, oldEntity, newEntity, in map);
						}
						else if (!GeneratedCopier.IsSkipped(typeId))
						{
							// Ссылочный компонент без маркера ([GenerateCopy]/[ManualCopy]/[SkipCopy]) при копии потеряется.
							// Сообщаем в код игры: в релизе лог виден, в DEBUG падаем. Список непокрытого - _worklist.txt.
							GeneratedCopier.ReportUnhandled(typeId);
						}
					}
				}

				return map[root];
			}
			finally
			{
				// Чистим временные буферы обхода. Полу-созданные сущности dstWorld не откатываем: throw здесь
				// рвёт загрузку этапа, мир целиком отбрасывается.
				if (map.IsCreated)
				{
					map.Dispose();
				}
				toCopy.Dispose();
				visited.Dispose();
				frontier.Dispose();
			}
		}

		/// <summary>
		/// Переводит старую сущность в новую по таблице копирования. Если сущности нет в таблице (ссылка
		/// на чужую сущность, которую не копировали) - возвращает <see cref="Entity.EMPTY"/>.
		/// </summary>
		public static Entity Remap(this in UnsafeDictionary<Entity, Entity> map, Entity entity)
		{
			return map.TryGetValue(entity, out var mapped) ? mapped : Entity.EMPTY;
		}

		/// <summary>
		/// Читает компонент <typeparamref name="T"/> из старого мира, делает копию значения и
		/// перенастраивает ссылки через <see cref="ICopiable{T}.InnerCopy"/>. Возвращает копию для записи
		/// в новый мир.
		/// </summary>
		public static T Copy<T>(this WorldState oldWS, Entity entity, WorldState newWS, in UnsafeDictionary<Entity, Entity> map)
			where T : unmanaged, IComponent, ICopiable<T>
		{
			var oldComponent = new ComponentSetContext<T>(oldWS).ReadElement(entity);
			var newComponent = oldComponent;
			oldComponent.InnerCopy(oldWS, newWS, ref newComponent, in map);
			return newComponent;
		}

		private static Entity CreateEntityCopy(WorldState srcWorld, WorldState dstWorld, Entity oldEntity)
		{
#if ENABLE_ENTITY_NAMES
			var name = srcWorld.GetService<EntityStatePart>().GetEntityName(srcWorld, oldEntity);
			return dstWorld.GetService<EntityStatePart>().CreateEntity(dstWorld, name);
#else
			return dstWorld.GetService<EntityStatePart>().CreateEntity(dstWorld);
#endif
		}
	}
}
