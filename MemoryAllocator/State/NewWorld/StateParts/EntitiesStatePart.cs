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
		public uint EntitiesCount { get; private set; }
		public uint EntitiesCapacity { get; private set; }
		public uint ExpandStep { get; private set; }

		private List<ushort> _freeEntitiesIds;
		private List<ushort> _entityIdToGeneration;
		private ProxyEvent<IEntityDestroySubscriberProxy> _entityDestroySubscribers;

#if UNITY_EDITOR
		public List<FixedString32Bytes> entityIdToName;
		public uint MaxEntitiesCount { get; private set; }
#endif

		public EntitiesStatePart(uint entitiesCapacity, uint expandStep = 512)
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
			ref var allocator = ref _allocatorId.GetAllocator();

			_freeEntitiesIds = new (ref allocator, EntitiesCapacity);
			_entityIdToGeneration = new(ref allocator, EntitiesCapacity);
			_entityDestroySubscribers = new(ref allocator, 128);
#if UNITY_EDITOR
			entityIdToName = new (ref allocator, EntitiesCapacity);
#endif

			for (ushort i = 0; i < EntitiesCapacity; i++)
			{
				_freeEntitiesIds[allocator, i] = i;
			}

#if UNITY_EDITOR
			SharedEntity = CreateEntity(ref _allocatorId.GetAllocator(), "SHARED");
#else
			SharedEntity = CreateEntity();
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
		public Entity CreateEntity(ref Allocator allocator, string name = null)
#else
		public Entity CreateEntity(ref Allocator allocator)

#endif
		{
			EnsureCapacity(EntitiesCount);

			var id = _freeEntitiesIds[allocator, EntitiesCount++];
			var generation = ++_entityIdToGeneration[allocator, id];

			var entity = new Entity(id, generation, allocator.allocatorId);
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
		public void DestroyEntity(Entity entity)
		{
			var allocator = _allocatorId.GetAllocatorPtr();
			Debug.Assert(IsEntityAlive(allocator, entity));

			//_entityDestroySubscribers.EntityDestroyed(allocator, entity);

			_entityIdToGeneration[allocator, entity.id]++;
			_freeEntitiesIds[allocator, --EntitiesCount] = entity.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DestroyEntity(Allocator* allocator, Entity entity)
		{
			Debug.Assert(IsEntityAlive(allocator, entity));

			//_entityDestroySubscribers.EntityDestroyed(allocator, entity);

			_entityIdToGeneration[allocator, entity.id]++;
			_freeEntitiesIds[allocator, --EntitiesCount] = entity.id;
		}

		private void EnsureCapacity(uint index)
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
