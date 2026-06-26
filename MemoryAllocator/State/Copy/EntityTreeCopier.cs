using System;
using Sapientia.Collections;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Обход и копирование поддеревьев сущностей в другой мир за один батч (один раз, при переходе между
	/// этапами). Три прохода: собрать все поддеревья (<see cref="CollectAll"/>); создать все копии и запомнить
	/// пары старая-новая (<see cref="CreateCopies"/>); скопировать значения и перенастроить ссылки
	/// (<see cref="CopyValues"/>). Три, а не два, потому что для перенастройки ссылки новая сущность уже должна
	/// существовать. Буферы off-arena, освобождаются в <see cref="Dispose"/>.
	/// </summary>
	public struct EntityTreeCopier : IDisposable
	{
		private readonly WorldState _srcWorld;
		private readonly WorldState _dstWorld;
		private readonly bool _hasIgnoreSet;

		private UnsafeList<Entity> _toCopy;
		private UnsafeBitArray _visited;
		private UnsafeList<Entity> _frontier;
		private UnsafeDictionary<Entity, Entity> _map;

		public EntityTreeCopier(WorldState srcWorld, WorldState dstWorld)
		{
			_srcWorld = srcWorld;
			_dstWorld = dstWorld;
			// Набор для метки может быть не зарегистрирован; тогда метки нет ни на одной сущности.
			_hasIgnoreSet = srcWorld.HasComponentSet<IgnoreEntityCopy>();

			// visited - бит-сет по entity.id (ushort): id всех живых сущностей исходного мира < EntitiesCapacity.
			var entitiesCapacity = srcWorld.GetService<EntityStatePart>().EntitiesCapacity;
			_toCopy = new UnsafeList<Entity>(16);
			_visited = new UnsafeBitArray(default, entitiesCapacity);
			_frontier = new UnsafeList<Entity>(16);
			_map = default;
		}

		/// <summary>
		/// Сеет корень в обход. Предусловие (до <see cref="CollectAll"/>): корень должен копироваться, иначе он
		/// не попадёт в таблицу и <see cref="GetCopy"/> упадёт. Пустой или помеченный IgnoreEntityCopy корень -
		/// нарушение, ловим явным E.ASSERT в DEBUG.
		/// </summary>
		public void AddRoot(Entity root)
		{
			E.ASSERT(!root.IsEmpty(), "EntityTreeCopier: root пустой - копировать нечего, нарушено предусловие.");
			E.ASSERT(!_hasIgnoreSet || !root.Has<IgnoreEntityCopy>(_srcWorld), "EntityTreeCopier: root помечен IgnoreEntityCopy - корень не скопируется, GetCopy упадёт. Нарушено предусловие.");
			_frontier.Add(root);
		}

		/// <summary>
		/// Проход A: обойти поддеревья всех засеянных корней (без повторов через visited), собрать в toCopy.
		/// </summary>
		public void CollectAll()
		{
			// Реестр компонентов: индекс слота = локальный индекс компонента, по нему идёт диспатч копира.
			ref var manager = ref _srcWorld.GetComponentsManager();

			while (_frontier.count > 0)
			{
				var entity = _frontier.RemoveLast();
				// Пустая ссылка (необязательная дочерняя сущность не задана) не копируется: иначе для неё
				// создалась бы лишняя сущность-копия, и перенастройка ссылок вернула бы её вместо Entity.EMPTY.
				if (entity.IsEmpty())
				{
					continue;
				}
				if (_visited.IsSet(entity.id))
				{
					continue;
				}
				_visited.Set(entity.id, true);
				if (_hasIgnoreSet && entity.Has<IgnoreEntityCopy>(_srcWorld))
				{
					continue;
				}

				_toCopy.Add(entity);

				for (var i = 0; i < manager.Length; i++)
				{
					ref var slot = ref manager.GetByIndex(i);
					if (!slot.IsValid())
					{
						continue;
					}
					if (!slot.GetPtr(_srcWorld).Value().HasElement(_srcWorld, entity))
					{
						continue;
					}
					if (GeneratedCopier.IsCopiable(i))
					{
						GeneratedCopier.AppendEntities(i, _srcWorld, entity, ref _frontier);
					}
				}
			}
		}

		/// <summary>
		/// Проход B: создать все копии, чтобы на проходе C ссылки уже было куда перенастраивать.
		/// </summary>
		public void CreateCopies()
		{
			_map = new UnsafeDictionary<Entity, Entity>(default, _toCopy.count);
			foreach (var oldEntity in _toCopy)
			{
				var newEntity = CreateEntityCopy(oldEntity);
				_map.Add(oldEntity, newEntity);
			}
		}

		/// <summary>
		/// Проход C: скопировать значения и перенастроить ссылки по таблице.
		/// </summary>
		public void CopyValues()
		{
			ref var manager = ref _srcWorld.GetComponentsManager();

			foreach (var oldEntity in _toCopy)
			{
				var newEntity = _map[oldEntity];

				for (var i = 0; i < manager.Length; i++)
				{
					ref var slot = ref manager.GetByIndex(i);
					if (!slot.IsValid())
					{
						continue;
					}
					if (!slot.GetPtr(_srcWorld).Value().HasElement(_srcWorld, oldEntity))
					{
						continue;
					}
					if (GeneratedCopier.IsCopiable(i))
					{
						GeneratedCopier.CopyComponent(i, _srcWorld, _dstWorld, oldEntity, newEntity, in _map);
					}
					else if (!GeneratedCopier.IsSkipped(i))
					{
						// Ссылочный компонент без маркера ([GenerateCopy]/[ManualCopy]/[SkipCopy]) при копии потеряется.
						// Сообщаем в код игры: в релизе лог виден, в DEBUG падаем. Список непокрытого - _worklist.txt.
						GeneratedCopier.ReportUnhandled(i);
					}
				}
			}
		}

		/// <summary>
		/// Возвращает копию корня (или любой собранной сущности) после прохода C.
		/// </summary>
		public Entity GetCopy(Entity entity)
		{
			return _map[entity];
		}

		public void Dispose()
		{
			// Полу-созданные сущности dstWorld не откатываем: throw где-либо в проходах рвёт загрузку этапа,
			// мир целиком отбрасывается. Чистим только временные буферы обхода.
			if (_map.IsCreated)
			{
				_map.Dispose();
			}
			_toCopy.Dispose();
			_visited.Dispose();
			_frontier.Dispose();
		}

		private Entity CreateEntityCopy(Entity oldEntity)
		{
#if ENABLE_ENTITY_NAMES
			ref readonly var name = ref _srcWorld.GetService<EntityStatePart>().GetEntityNameRef(_srcWorld, oldEntity);
			return _dstWorld.GetService<EntityStatePart>().CreateEntity(_dstWorld, in name);
#else
			return _dstWorld.GetService<EntityStatePart>().CreateEntity(_dstWorld);
#endif
		}
	}
}
