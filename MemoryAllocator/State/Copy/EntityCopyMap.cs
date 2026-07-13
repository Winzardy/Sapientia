using Sapientia.Collections;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Отображение сущностей старого мира в их копии в новом (копия = тот же id+generation, новый worldId).
	/// Таблица generation'ов вставленных id (0 = не вставлен) вместо словаря пар: ссылка на
	/// не-скопированное или протухшая даёт EMPTY. Указатель на буфер <see cref="EntityTreeCopier"/> - не хранить дольше него.
	/// </summary>
	public readonly struct EntityCopyMap
	{
		private readonly UnsafeArray<ushort> _insertedGenerations;
		private readonly WorldId _oldWorldId;
		private readonly WorldId _newWorldId;

		public int Count { get; }

		internal EntityCopyMap(UnsafeArray<ushort> insertedGenerations, int count, WorldId oldWorldId, WorldId newWorldId)
		{
			_insertedGenerations = insertedGenerations;
			_oldWorldId = oldWorldId;
			_newWorldId = newWorldId;
			Count = count;
		}

		public Entity GetOrDefault(in Entity oldEntity)
		{
			TryGetValue(oldEntity, out var newEntity);
			return newEntity;
		}

		public bool TryGetValue(in Entity oldEntity, out Entity newEntity)
		{
			newEntity = Entity.EMPTY;
			// Проверка по generation: пустая ссылка с ненулевым id на невставленном слоте дала бы ложное 0 == 0.
			if (oldEntity.IsEmpty())
			{
				return false;
			}
			// Словарь ключевался полным Entity (worldId входит в ==) - ссылку не из старого мира ловим явно.
			E.ASSERT(oldEntity.worldId == _oldWorldId, "EntityCopyMap: ссылка не из старого мира.");
			// После worldId-ассерта выше честная ссылка старого мира не выйдет за границы таблицы -
			// её id всегда < EntitiesCapacity на момент захвата (см. EntityTreeCopier).
			E.ASSERT(oldEntity.id < _insertedGenerations.Length, "EntityCopyMap: id ссылки за пределами таблицы пар.");
			if (_insertedGenerations[oldEntity.id] != oldEntity.generation)
			{
				return false;
			}

			newEntity = new Entity(oldEntity.id, oldEntity.generation, _newWorldId);
			return true;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(in this);
		}

		public struct Entry
		{
			public Entity key;
			public Entity value;
		}

		public struct Enumerator
		{
			private readonly EntityCopyMap _map;
			private Entry _current;
			private int _id;

			internal Enumerator(in EntityCopyMap map)
			{
				_map = map;
				_current = default;
				_id = -1;
			}

			// По значению: пары генерятся на лету, а ref на своё поле struct отдать не может (CS8170).
			public readonly Entry Current => _current;

			public bool MoveNext()
			{
				var length = _map._insertedGenerations.Length;
				while (++_id < length)
				{
					var generation = _map._insertedGenerations[_id];
					if (generation == Entity.GENERATION_ZERO)
					{
						continue;
					}

					_current = new Entry
					{
						key = new Entity((ushort)_id, generation, _map._oldWorldId),
						value = new Entity((ushort)_id, generation, _map._newWorldId),
					};
					return true;
				}
				return false;
			}
		}
	}
}
