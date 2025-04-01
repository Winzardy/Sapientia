using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Collections.FixedString;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe interface IEntityDestroySubscriber : IInterfaceProxyType
	{
		public void EntityArrayDestroyed(SafePtr<Allocator> allocator, Entity* entities, int count);
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

#if UNITY_EDITOR
		public MemArray<FixedString64Bytes> entityIdToName;
		public int MaxEntitiesCount { get; private set; }

		public string GetEntityName(SafePtr<Allocator> allocator, in Entity entity)
		{
			return entityIdToName[allocator, entity.id].ToString();
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
#if UNITY_EDITOR
			entityIdToName = default;
			MaxEntitiesCount = 0;
#endif
		}

		public void Initialize(SafePtr<Allocator> allocator, IndexedPtr self)
		{
			_freeEntitiesIds = new MemArray<ushort>(allocator, EntitiesCapacity, ClearOptions.UninitializedMemory);
			_entityIdToGeneration = new MemArray<ushort>(allocator, EntitiesCapacity);
			_entityDestroySubscribers = new ProxyEvent<IEntityDestroySubscriberProxy>(allocator, 128);
#if UNITY_EDITOR
			entityIdToName = new MemArray<FixedString64Bytes>(allocator, EntitiesCapacity);
#endif

			for (ushort i = 0; i < EntitiesCapacity; i++)
			{
				_freeEntitiesIds[allocator, i] = i;
			}

#if UNITY_EDITOR
			WorldEntity = CreateEntity(allocator, "World");
#else
			WorldEntity = CreateEntity(allocator);
#endif
		}

		public void AddSubscriber(SafePtr<Allocator> allocator, in ProxyPtr<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.Subscribe(allocator, subscriber);
		}

		public void AddSubscriber(in ProxyPtr<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.Subscribe(subscriber);
		}

		public void RemoveSubscriber(in ProxyPtr<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.UnSubscribe(subscriber);
		}

		public void RemoveSubscriber(SafePtr<Allocator> allocator, in ProxyPtr<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.UnSubscribe(allocator, subscriber);
		}

#if UNITY_EDITOR
		public Entity CreateEntity(SafePtr<Allocator> allocator, in FixedString64Bytes name = default)
#else
		public Entity CreateEntity(SafePtr<Allocator> allocator)

#endif
		{
			EnsureCapacity(allocator, EntitiesCount);

			var id = _freeEntitiesIds[allocator, EntitiesCount++];
			var generation = ++_entityIdToGeneration[allocator, id];

			var entity = new Entity(id, generation, allocator.Value().allocatorId);
#if UNITY_EDITOR
			entityIdToName[allocator, id] = name;
			if (MaxEntitiesCount < EntitiesCount)
				MaxEntitiesCount = EntitiesCount;
#endif
			return entity;
		}

		public bool IsEntityExist(SafePtr<Allocator> allocator, in Entity entity)
		{
			return entity.id < EntitiesCapacity && _entityIdToGeneration[allocator, entity.id] == entity.generation;
		}

		public bool IsEntityExist(in Entity entity)
		{
			return entity.id < EntitiesCapacity && _entityIdToGeneration[entity.id] == entity.generation;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DestroyEntities(SafePtr<Allocator> allocator, Entity* entities, int count)
		{
#if DEBUG
			for (var i = 0; i < count; i++)
			{
				Debug.Assert(IsEntityExist(allocator, entities[i]));
			}

#endif
			_entityDestroySubscribers.EntityArrayDestroyed(allocator, allocator, entities, count);

			for (var i = 0; i < count; i++)
			{
				var entityId = entities[i].id;

				_entityIdToGeneration[allocator, entityId]++;
				_freeEntitiesIds[allocator, --EntitiesCount] = entityId;
			}
		}

		private void EnsureCapacity(SafePtr<Allocator> allocator, int index)
		{
			if (index < EntitiesCapacity)
				return;

			do
				EntitiesCapacity += ExpandStep;
			while (index >= EntitiesCapacity);

			_freeEntitiesIds.Resize(allocator, EntitiesCapacity);
			_entityIdToGeneration.Resize(allocator, EntitiesCapacity);
#if UNITY_EDITOR
			entityIdToName.Resize(allocator, EntitiesCapacity);
#endif
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
			UnityEngine.Debug.LogWarning($"Entities Capacity was expanded to {EntitiesCapacity}");
#endif
		}
	}
}
