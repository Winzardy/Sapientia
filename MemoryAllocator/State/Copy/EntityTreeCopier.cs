using System;
using Sapientia.Collections;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Обход и копирование поддеревьев сущностей в другой мир за один батч (один раз, при переходе между
	/// этапами). Копия сохраняет id+generation оригинала: id вставляются в новый мир в обход free-list
	/// (<see cref="EntityStatePart.InsertEntity"/>), поэтому вставка возможна только пока новый мир не начал
	/// раздавать id сам. Три прохода: собрать все поддеревья (<see cref="CollectAll"/>); вставить все копии
	/// и зафиксировать пары старая-новая (<see cref="InsertCopies"/>); скопировать значения и перенастроить
	/// ссылки (<see cref="CopyValues"/>). Буферы off-arena, освобождаются в <see cref="Dispose"/>.
	/// </summary>
	public struct EntityTreeCopier : IDisposable
	{
		private readonly WorldState _srcWorld;
		private readonly WorldState _dstWorld;
		private readonly bool _hasIgnoreSet;

		private UnsafeList<Entity> _toCopy;
		private UnsafeBitArray _visited;
		private UnsafeList<Entity> _frontier;
		// Таблица пар: индекс = id старого мира, значение = generation вставленной копии (0 = не вставлен).
		private UnsafeArray<ushort> _insertedGenerations;
		private int _pairsCount;

		public bool IsCreated => _toCopy.IsCreated;

		/// <summary>
		/// Все пары старая-новая после <see cref="InsertCopies"/> (включая засеянные через
		/// <see cref="SeedExisting"/>). Хендл на буфер копира - не хранить дольше него.
		/// </summary>
		public EntityCopyMap Map => new(_insertedGenerations, _pairsCount, _srcWorld.WorldId, _dstWorld.WorldId);

		public EntityTreeCopier(WorldState srcWorld, WorldState dstWorld)
		{
			_srcWorld = srcWorld;
			_dstWorld = dstWorld;
			// Набор для метки может быть не зарегистрирован; тогда метки нет ни на одной сущности.
			_hasIgnoreSet = srcWorld.HasComponentSet<IgnoreEntityCopy>();

			// visited и таблица пар - по entity.id (ushort): id всех живых сущностей исходного мира < EntitiesCapacity.
			var entitiesCapacity = srcWorld.GetService<EntityStatePart>().EntitiesCapacity;
			_toCopy = new UnsafeList<Entity>(16);
			_visited = new UnsafeBitArray(entitiesCapacity);
			_frontier = new UnsafeList<Entity>(16);
			_insertedGenerations = new UnsafeArray<ushort>(entitiesCapacity);
			_pairsCount = 0;
		}

		/// <summary>
		/// Сеет корень в обход. Предусловие (до <see cref="CollectAll"/>): корень должен копироваться, иначе он
		/// не попадёт в таблицу пар и ссылки на него в перенесённых компонентах умрут в EMPTY. Пустой или
		/// помеченный IgnoreEntityCopy корень - нарушение, ловим явным E.ASSERT в DEBUG.
		/// </summary>
		public void AddRoot(Entity root)
		{
			E.ASSERT(!root.IsEmpty(), "EntityTreeCopier: root пустой - копировать нечего, нарушено предусловие.");
			E.ASSERT(!_hasIgnoreSet || !root.Has<IgnoreEntityCopy>(_srcWorld), "EntityTreeCopier: root помечен IgnoreEntityCopy - корень не скопируется, ссылки на него умрут в EMPTY. Нарушено предусловие.");
			_frontier.Add(root);
		}

		/// <summary>
		/// Вписывает в таблицу пар сущность, копия которой уже существует в новом мире под теми же
		/// id+generation (например World - его создаёт инициализация нового мира, вставлять нельзя и не нужно).
		/// Помечает oldEntity visited, чтобы CollectAll не собрал её при встрече по ссылке.
		/// Только до <see cref="InsertCopies"/>.
		/// </summary>
		public void SeedExisting(Entity oldEntity)
		{
			E.ASSERT(!oldEntity.IsEmpty(), "EntityTreeCopier: SeedExisting oldEntity пустой.");
			E.ASSERT(_dstWorld.GetService<EntityStatePart>().IsEntityExist(_dstWorld, new Entity(oldEntity.id, oldEntity.generation, _dstWorld.WorldId)),
				"EntityTreeCopier: SeedExisting - в новом мире нет живой сущности с теми же id+generation.");

			_insertedGenerations[oldEntity.id] = oldEntity.generation;
			_pairsCount++;
			_visited.Set(oldEntity.id, true);
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
		/// Проход B: вставить все копии в новый мир под старыми id+generation, чтобы на проходе C ссылки
		/// уже было куда перенастраивать. Завершается одним пересбором free-list нового мира
		/// (<see cref="EntityStatePart.ResetFreeIndexes"/>) - до него создание сущностей в новом мире закрыто.
		/// </summary>
		public void InsertCopies()
		{
			ref var dstEntityStatePart = ref _dstWorld.GetService<EntityStatePart>();
#if ENABLE_ENTITY_NAMES
			ref var srcEntityStatePart = ref _srcWorld.GetService<EntityStatePart>();
#endif

			foreach (var oldEntity in _toCopy)
			{
#if ENABLE_ENTITY_NAMES
				ref readonly var name = ref srcEntityStatePart.GetEntityNameRef(_srcWorld, oldEntity);
				dstEntityStatePart.InsertEntity(_dstWorld, oldEntity.id, oldEntity.generation, in name);
#else
				dstEntityStatePart.InsertEntity(_dstWorld, oldEntity.id, oldEntity.generation);
#endif
				_insertedGenerations[oldEntity.id] = oldEntity.generation;
				_pairsCount++;
			}

			dstEntityStatePart.ResetFreeIndexes(_dstWorld);
		}

		/// <summary>
		/// Проход C: скопировать значения и перенастроить ссылки по таблице.
		/// </summary>
		public void CopyValues()
		{
			ref var manager = ref _srcWorld.GetComponentsManager();
			var map = Map;

			foreach (var oldEntity in _toCopy)
			{
				var newEntity = new Entity(oldEntity.id, oldEntity.generation, _dstWorld.WorldId);

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
						GeneratedCopier.CopyComponent(i, _srcWorld, _dstWorld, oldEntity, newEntity, in map);
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

		public void Dispose()
		{
			// Полу-вставленные сущности dstWorld не откатываем: throw где-либо в проходах рвёт загрузку этапа,
			// мир целиком отбрасывается. Чистим только временные буферы обхода.
			_toCopy.Dispose();
			_visited.Dispose();
			_frontier.Dispose();
			_insertedGenerations.Dispose();
		}
	}
}
