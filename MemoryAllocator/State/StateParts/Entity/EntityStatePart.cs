using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Collections.FixedString;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe interface IEntityDestroySubscriber : IInterfaceProxyType
	{
		public void EntityArrayDestroyed(World world, Entity* entities, int count);
	}

	public unsafe struct EntityStatePart : IWorldStatePart
	{
		public Entity WorldEntity { get; private set; }
		public int EntitiesCount { get; private set; }
		public int EntitiesCapacity { get; private set; }
		public int ExpandStep { get; private set; }

		private MemArray<ushort> _freeEntitiesIds;
		private MemArray<ushort> _entityIdToGeneration;
		private ProxyEvent<IEntityDestroySubscriberProxy> _entityDestroySubscribers;

#if ENABLE_ENTITY_NAMES
		public MemArray<FixedString64Bytes> entityIdToName;
		public int MaxEntitiesCount { get; private set; }

		public string GetEntityName(World world, in Entity entity)
		{
			if (!IsEntityExist(entity))
				return "[Destroyed]";
			return entityIdToName[world, entity.id].ToString();
		}
#endif

		public EntityStatePart(int entitiesCapacity, int expandStep = 512)
		{
			WorldEntity = default;
			EntitiesCount = 0;
			EntitiesCapacity = entitiesCapacity;
			ExpandStep = expandStep;

			_freeEntitiesIds = default;
			_entityIdToGeneration = default;
			_entityDestroySubscribers = default;
#if ENABLE_ENTITY_NAMES
			entityIdToName = default;
			MaxEntitiesCount = 0;
#endif
		}

		public void Initialize(World world, IndexedPtr self)
		{
			_freeEntitiesIds = new MemArray<ushort>(world, EntitiesCapacity, ClearOptions.UninitializedMemory);
			_entityIdToGeneration = new MemArray<ushort>(world, EntitiesCapacity);
			_entityDestroySubscribers = new ProxyEvent<IEntityDestroySubscriberProxy>(world, 256);
#if ENABLE_ENTITY_NAMES
			entityIdToName = new MemArray<FixedString64Bytes>(world, EntitiesCapacity);
#endif

			for (ushort i = 0; i < EntitiesCapacity; i++)
			{
				_freeEntitiesIds[world, i] = i;
			}

#if ENABLE_ENTITY_NAMES
			WorldEntity = CreateEntity(world, "World");
#else
			WorldEntity = CreateEntity(allocator);
#endif
		}

		public void AddSubscriber(World world, in ProxyPtr<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.Subscribe(world, subscriber);
		}

		public void AddSubscriber(in ProxyPtr<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.Subscribe(subscriber);
		}

		public void RemoveSubscriber(in ProxyPtr<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.UnSubscribe(subscriber);
		}

		public void RemoveSubscriber(World world, in ProxyPtr<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.UnSubscribe(world, subscriber);
		}

#if ENABLE_ENTITY_NAMES
		public Entity CreateEntity(World world, in FixedString64Bytes name = default)
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity CreateEntity(Allocator allocator, in FixedString64Bytes name)
		{
			return CreateEntity(allocator);
		}

		public Entity CreateEntity(Allocator allocator, string name = null)
#endif
		{
			EnsureCapacity(world, EntitiesCount);

			var id = _freeEntitiesIds[world, EntitiesCount++];
			var generation = ++_entityIdToGeneration[world, id];

			var entity = new Entity(id, generation, world.worldId);
#if ENABLE_ENTITY_NAMES
			entityIdToName[world, id] = name;
			if (MaxEntitiesCount < EntitiesCount)
				MaxEntitiesCount = EntitiesCount;
#endif
			return entity;
		}

		public bool IsEntityExist(World world, in Entity entity)
		{
			return entity.id < EntitiesCapacity && _entityIdToGeneration[world, entity.id] == entity.generation;
		}

		public bool IsEntityExist(in Entity entity)
		{
			return entity.id < EntitiesCapacity && _entityIdToGeneration[entity.id] == entity.generation;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DestroyEntities(World world, Entity* entities, int count)
		{
#if DEBUG
			for (var i = 0; i < count; i++)
			{
				E.ASSERT(IsEntityExist(world, entities[i]));
			}

#endif
			_entityDestroySubscribers.EntityArrayDestroyed(world, world, entities, count);

			for (var i = 0; i < count; i++)
			{
				var entityId = entities[i].id;

				_entityIdToGeneration[world, entityId]++;
				_freeEntitiesIds[world, --EntitiesCount] = entityId;
			}
		}

		private void EnsureCapacity(World world, int index)
		{
			if (index < EntitiesCapacity)
				return;

			do
				EntitiesCapacity += ExpandStep;
			while (index >= EntitiesCapacity);

			var freeStartIndex = _freeEntitiesIds.Length;

			_freeEntitiesIds.Resize(world, EntitiesCapacity, ClearOptions.UninitializedMemory);
			_entityIdToGeneration.Resize(world, EntitiesCapacity);
#if ENABLE_ENTITY_NAMES
			entityIdToName.Resize(world, EntitiesCapacity);
#endif
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
			UnityEngine.Debug.LogWarning($"Entities Capacity was expanded to {EntitiesCapacity}");
#endif

			for (ushort i = (ushort)freeStartIndex; i < EntitiesCapacity; i++)
			{
				_freeEntitiesIds[world, i] = i;
			}
		}
	}
}
