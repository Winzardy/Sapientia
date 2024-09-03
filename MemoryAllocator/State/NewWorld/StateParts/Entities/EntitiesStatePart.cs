using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Collections.Fixed;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	[InterfaceProxy]
	public unsafe interface IEntityDestroySubscriber
	{
		public void EntityDestroyed(Allocator* allocator, in Entity entity);
	}

	public unsafe struct EntitiesStatePart : IWorldStatePart
	{
		private AllocatorId _allocatorId;
		AllocatorId IWorldElement.AllocatorId
		{
			get => _allocatorId;
			set => _allocatorId = value;
		}

		public Entity SharedEntity { get; private set; }
		public int EntitiesCount { get; private set; }
		public int EntitiesCapacity { get; private set; }
		public int ExpandStep { get; private set; }

		private List<ushort> _freeEntitiesIds;
		private List<ushort> _entityIdToGeneration;
		private ProxyEvent<IEntityDestroySubscriberProxy> _entityDestroySubscribers;

#if UNITY_EDITOR
		public List<FixedString32Bytes> entityIdToName;
		public int MaxEntitiesCount { get; private set; }
#endif

		public EntitiesStatePart(int entitiesCapacity, int expandStep = 512)
		{
			_allocatorId = default;

			SharedEntity = default;
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

		public void Initialize()
		{
			var allocator = _allocatorId.GetAllocatorPtr();

			_freeEntitiesIds = new (allocator, EntitiesCapacity);
			_entityIdToGeneration = new(allocator, EntitiesCapacity);
			_entityDestroySubscribers = new(allocator, 128);
#if UNITY_EDITOR
			entityIdToName = new (allocator, EntitiesCapacity);
#endif

			for (ushort i = 0; i < EntitiesCapacity; i++)
			{
				_freeEntitiesIds[allocator, i] = i;
			}

#if UNITY_EDITOR
			SharedEntity = CreateEntity(allocator, "SHARED");
#else
			SharedEntity = CreateEntity(allocator);
#endif
		}

		public void AddSubscriber(in ProxyRef<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.Subscribe(subscriber);
		}

		public void RemoveSubscriber(in ProxyRef<IEntityDestroySubscriberProxy> subscriber)
		{
			_entityDestroySubscribers.UnSubscribe(subscriber);
		}

#if UNITY_EDITOR
		public Entity CreateEntity(Allocator* allocator, string name = null)
#else
		public Entity CreateEntity(Allocator* allocator)

#endif
		{
			EnsureCapacity(EntitiesCount);

			var id = _freeEntitiesIds[allocator, EntitiesCount++];
			var generation = ++_entityIdToGeneration[allocator, id];

			var entity = new Entity(id, generation, allocator->allocatorId);
#if UNITY_EDITOR
			entityIdToName[allocator, id] = name;
			if (MaxEntitiesCount < EntitiesCount)
				MaxEntitiesCount = EntitiesCount;
#endif

			return entity;
		}

		public bool IsEntityAlive(Allocator* allocator, in Entity entity)
		{
			return _entityIdToGeneration[allocator, entity.id] == entity.generation;
		}

		public bool IsEntityAlive(in Entity entity)
		{
			return _entityIdToGeneration[entity.id] == entity.generation;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DestroyEntity(in Entity entity)
		{
			var allocator = _allocatorId.GetAllocatorPtr();
			DestroyEntity(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DestroyEntity(Allocator* allocator, in Entity entity)
		{
			Debug.Assert(IsEntityAlive(allocator, entity));

			_entityDestroySubscribers.EntityDestroyed(allocator, allocator, entity);

			_entityIdToGeneration[allocator, entity.id]++;
			_freeEntitiesIds[allocator, --EntitiesCount] = entity.id;
		}

		private void EnsureCapacity(int index)
		{
			if (index < EntitiesCapacity)
				return;

			do
				EntitiesCapacity += ExpandStep;
			while (index >= EntitiesCapacity);

			_freeEntitiesIds.EnsureCapacity(EntitiesCapacity);
			_entityIdToGeneration.EnsureCapacity(EntitiesCapacity);
#if UNITY_EDITOR
			entityIdToName.EnsureCapacity(EntitiesCapacity);
#endif
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && ARCHETYPES_DEBUG)
			UnityEngine.Debug.LogWarning($"Entities Capacity was expanded to {EntitiesCapacity}");
#endif
		}
	}
}
