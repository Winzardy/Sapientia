using System;
using System.Runtime.CompilerServices;
using Sapientia.Collections.FixedString;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe interface IEntityDestroySubscriber : IInterfaceProxyType
	{
		public void EntityArrayDestroyed(WorldState worldState, Entity* entities, int count);
	}

	public unsafe struct EntityStatePart : IWorldStatePart
	{
		public Entity WorldEntity => worldEntity;
		public int EntitiesCount { get; private set; }
		public int EntitiesCapacity { get; private set; }
		public int ExpandStep { get; private set; }

		internal Entity worldEntity;

		private MemArray<ushort> _freeEntitiesIds;
		private MemArray<ushort> _entityIdToGeneration;
		private ProxyEvent<IEntityDestroySubscriberProxy> _entityDestroySubscribers;
#if DEBUG
		private MemSparseSet<Entity> _aliveEntities;
		private bool _freeIndexesDirty;
#endif

#if ENABLE_ENTITY_NAMES
		public MemArray<FixedString64Bytes> entityIdToName;
		public int MaxEntitiesCount { get; private set; }

		public string GetEntityName(WorldState worldState, in Entity entity)
		{
			if (!IsEntityExist(worldState, entity))
				return "[Destroyed]";
			return entityIdToName[worldState, entity.id].ToString();
		}

		/// <summary>
		/// Имя сущности без конвертации в string (индексер MemArray даёт ref). Для копирования имени между
		/// мирами без round-trip FixedString-string-FixedString. Сущность должна быть живой.
		/// </summary>
		public ref readonly FixedString64Bytes GetEntityNameRef(WorldState worldState, in Entity entity)
		{
			E.ASSERT(IsEntityExist(worldState, entity), "GetEntityNameRef: сущность уже уничтожена.");
			return ref entityIdToName[worldState, entity.id];
		}
#endif

		public EntityStatePart(int entitiesCapacity, int expandStep = 512)
		{
			worldEntity = default;
			EntitiesCount = 0;
			EntitiesCapacity = entitiesCapacity;
			ExpandStep = expandStep;

			_freeEntitiesIds = default;
			_entityIdToGeneration = default;
			_entityDestroySubscribers = default;
#if DEBUG
			_aliveEntities = default;
			_freeIndexesDirty = false;
#endif
#if ENABLE_ENTITY_NAMES
			entityIdToName = default;
			MaxEntitiesCount = 0;
#endif
		}

#if DEBUG
		public Span<Entity> GetAliveEntities(WorldState worldState)
		{
			return _aliveEntities.GetSpan(worldState);
		}
#endif

		public void Initialize(WorldState worldState, IndexedPtr self)
		{
			_freeEntitiesIds = new MemArray<ushort>(worldState, EntitiesCapacity, ClearOptions.UninitializedMemory);
			_entityIdToGeneration = new MemArray<ushort>(worldState, EntitiesCapacity);
			_entityDestroySubscribers = new ProxyEvent<IEntityDestroySubscriberProxy>(worldState, 256);
#if DEBUG
			_aliveEntities = new MemSparseSet<Entity>(worldState, EntitiesCapacity, EntitiesCapacity);
#endif
#if ENABLE_ENTITY_NAMES
			entityIdToName = new MemArray<FixedString64Bytes>(worldState, EntitiesCapacity);
#endif

			for (ushort i = 0; i < EntitiesCapacity; i++)
			{
				_freeEntitiesIds[worldState, i] = i;
			}

#if ENABLE_ENTITY_NAMES
			worldEntity = CreateEntity(worldState, "World");
#else
			worldEntity = CreateEntity(worldState);
#endif
		}

		public void AddSubscriber(WorldState worldState, in ProxyPtr<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.Subscribe(worldState, subscriber);
		}

		public void RemoveSubscriber(WorldState worldState, in ProxyPtr<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.UnSubscribe(worldState, subscriber);
		}

#if ENABLE_ENTITY_NAMES
		public Entity CreateEntity(WorldState worldState, in FixedString64Bytes name = default)
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity CreateEntity(WorldState worldState, in FixedString64Bytes name)
		{
			return CreateEntity(worldState);
		}

		public Entity CreateEntity(WorldState worldState, string name = null)
#endif
		{
#if DEBUG
			E.ASSERT(!_freeIndexesDirty, "CreateEntity: free-list недостоверен после InsertEntity - сначала ResetFreeIndexes.");
#endif
			EnsureCapacity(worldState, EntitiesCount);

			var id = _freeEntitiesIds[worldState, EntitiesCount++];
			var generation = ++_entityIdToGeneration[worldState, id];

			var entity = new Entity(id, generation, worldState.WorldId);
#if DEBUG
			_aliveEntities.EnsureGet(worldState, entity.id) = entity;
#endif

#if ENABLE_ENTITY_NAMES
			entityIdToName[worldState, id] = name;
			if (MaxEntitiesCount < EntitiesCount)
				MaxEntitiesCount = EntitiesCount;
#endif
			return entity;
		}

		/// <summary>
		/// Занимает конкретный id с конкретным generation в обход free-list (перенос сущностей в новый мир
		/// с сохранением id). Generation обязан быть нечётным - как у любой живой сущности. После серии
		/// вставок и до любого CreateEntity/DestroyEntities обязателен <see cref="ResetFreeIndexes"/>.
		/// </summary>
#if ENABLE_ENTITY_NAMES
		public Entity InsertEntity(WorldState worldState, ushort id, ushort generation, in FixedString64Bytes name = default)
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity InsertEntity(WorldState worldState, ushort id, ushort generation, in FixedString64Bytes name)
		{
			return InsertEntity(worldState, id, generation);
		}

		public Entity InsertEntity(WorldState worldState, ushort id, ushort generation, string name = null)
#endif
		{
			E.ASSERT(_entityIdToGeneration.IsCreated, "InsertEntity: часть ещё не инициализирована.");
			E.ASSERT(((int)generation).IsOdd(), "InsertEntity: generation чётный - таким может быть только мёртвая сущность.");

			EnsureCapacity(worldState, id);
			E.ASSERT(((int)_entityIdToGeneration[worldState, id]).IsEven(), "InsertEntity: id уже занят живой сущностью.");

			_entityIdToGeneration[worldState, id] = generation;
			EntitiesCount++;

			var entity = new Entity(id, generation, worldState.WorldId);
#if DEBUG
			_freeIndexesDirty = true;
			_aliveEntities.EnsureGet(worldState, entity.id) = entity;
#endif

#if ENABLE_ENTITY_NAMES
			entityIdToName[worldState, id] = name;
			if (MaxEntitiesCount < EntitiesCount)
				MaxEntitiesCount = EntitiesCount;
#endif
			return entity;
		}

		/// <summary>
		/// Пересобирает free-list и EntitiesCount сканом generation'ов после серии <see cref="InsertEntity"/>.
		/// Живость по чётности: create и destroy инкрементят generation по одному разу, поэтому у живой
		/// сущности generation всегда нечётный, у мёртвой/несозданной - чётный.
		/// </summary>
		public void ResetFreeIndexes(WorldState worldState)
		{
			var aliveCount = 0;
			var freeIndex = EntitiesCapacity;
			for (var id = EntitiesCapacity - 1; id >= 0; id--)
			{
				int generation = _entityIdToGeneration[worldState, id];
				if (generation.IsOdd())
				{
					aliveCount++;
				}
				else
				{
					_freeEntitiesIds[worldState, --freeIndex] = (ushort)id;
				}
			}

			E.ASSERT(aliveCount == EntitiesCount, "ResetFreeIndexes: число живых по generation-скану разошлось с EntitiesCount.");
#if DEBUG
			E.ASSERT(aliveCount == _aliveEntities.Count, "ResetFreeIndexes: generation-скан разошёлся с DEBUG-набором живых сущностей.");
			_freeIndexesDirty = false;
#endif
			EntitiesCount = aliveCount;
		}

		public bool IsEntityExist(WorldState worldState, in Entity entity)
		{
			return entity.id < EntitiesCapacity && _entityIdToGeneration[worldState, entity.id] == entity.generation;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DestroyEntities(WorldState worldState, Entity* entities, int count)
		{
#if DEBUG
			E.ASSERT(!_freeIndexesDirty, "DestroyEntities: free-list недостоверен после InsertEntity - сначала ResetFreeIndexes.");
			for (var i = 0; i < count; i++)
			{
				E.ASSERT(IsEntityExist(worldState, entities[i]), "Попытка уничтожить уже мёртвую entity!");
			}
#endif
			_entityDestroySubscribers.EntityArrayDestroyed(worldState, worldState, entities, count);

			for (var i = 0; i < count; i++)
			{
				var entityId = entities[i].id;

				_entityIdToGeneration[worldState, entityId]++;
				_freeEntitiesIds[worldState, --EntitiesCount] = entityId;
#if DEBUG
				_aliveEntities.RemoveSwapBack(worldState, entityId);
#endif
			}
		}

		private void EnsureCapacity(WorldState worldState, int index)
		{
			if (index < EntitiesCapacity)
				return;

			do
				EntitiesCapacity += ExpandStep;
			while (index >= EntitiesCapacity);

			var freeStartIndex = _freeEntitiesIds.Length;

			_freeEntitiesIds.Resize(worldState, EntitiesCapacity, ClearOptions.UninitializedMemory);
			_entityIdToGeneration.Resize(worldState, EntitiesCapacity);
#if ENABLE_ENTITY_NAMES
			entityIdToName.Resize(worldState, EntitiesCapacity);
#endif
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
			UnityEngine.Debug.LogWarning($"Entities Capacity was expanded to {EntitiesCapacity}");
#endif

			for (ushort i = (ushort)freeStartIndex; i < EntitiesCapacity; i++)
			{
				_freeEntitiesIds[worldState, i] = i;
			}
		}
	}

	public static class EntityStatePartExt
	{
		public static ReadOnlySpan<Entity> GetWorldEntitySpan(this ref EntityStatePart entityStatePart)
		{
			return entityStatePart.worldEntity.AsSpan();
		}
	}
}
