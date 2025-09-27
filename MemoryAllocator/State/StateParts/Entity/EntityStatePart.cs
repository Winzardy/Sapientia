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

		public bool IsEntityExist(WorldState worldState, in Entity entity)
		{
			return entity.id < EntitiesCapacity && _entityIdToGeneration[worldState, entity.id] == entity.generation;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DestroyEntities(WorldState worldState, Entity* entities, int count)
		{
#if DEBUG
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
