using System;
using System.Diagnostics;
using Sapientia.Extensions;

namespace Sapientia.Collections.Archetypes
{
	public static class EntitiesStateExt
	{
		public static bool IsExist(this in Entity entity)
		{
			return ServiceLocator<EntitiesState>.Instance.IsEntityAlive(entity);
		}

		public static void Destroy(this in Entity entity)
		{
			ServiceLocator<EntitiesState>.Instance.DestroyEntity(entity);
		}
	}

	public class EntitiesState : WorldStatePart
	{
		public static EntitiesState Instance => ServiceLocator<EntitiesState>.Instance;

		public Entity SharedEntity { get; private set; }

		public event Action<Entity> EntityDestroyEvent;

		private readonly SimpleList<ushort> _freeEntitiesIds;
		private readonly SimpleList<ushort> _entityIdToGeneration;
#if UNITY_EDITOR
		public readonly SimpleList<string> entitiesNames;

		public int MaxEntitiesCount { get; private set; }
#endif
		public int EntitiesCount { get; private set; }
		public int EntitiesCapacity { get; private set; }
		public int ExpandStep { get; private set; }

#if STORE_ENTITIES
		private readonly Archetype _entities;
		public ref readonly ArchetypeElement<EmptyValue>[] Entities => ref _entities.Elements;
#endif

		public EntitiesState(int entitiesCapacity, int expandStep = 512)
		{
#if UNITY_EDITOR
			MaxEntitiesCount = 0;
#endif
			EntitiesCount = 0;
			EntitiesCapacity = entitiesCapacity;
			ExpandStep = expandStep;

#if STORE_ENTITIES
			_entities = new (entitiesCapacity, entitiesCapacity);
#endif
			_freeEntitiesIds = new (entitiesCapacity);
			_entityIdToGeneration = new(entitiesCapacity, 0);
#if UNITY_EDITOR
			entitiesNames = new (entitiesCapacity);
#endif

			for (ushort i = 0; i < entitiesCapacity; i++)
			{
				_freeEntitiesIds[i] = i;
			}

#if UNITY_EDITOR
			SharedEntity = CreateEntity("SHARED");
#else
			SharedEntity = CreateEntity();
#endif
		}

#if UNITY_EDITOR
		public Entity CreateEntity(string name = null)
#else
		public Entity CreateEntity()

#endif
		{
			EnsureCapacity(EntitiesCount);

			var id = _freeEntitiesIds[EntitiesCount++];
			var generation = ++_entityIdToGeneration[id];

			var entity = new Entity(id, generation);
#if UNITY_EDITOR
			entitiesNames[id] = name;
			if (MaxEntitiesCount < EntitiesCount)
				MaxEntitiesCount = EntitiesCount;
#endif
#if STORE_ENTITIES
			_entities.GetElement(entity);
#endif

			return entity;
		}

		public bool IsEntityAlive(in Entity entity)
		{
			return _entityIdToGeneration[entity.id] == entity.generation;
		}

		public void DestroyEntity(in Entity entity)
		{
			Debug.Assert(IsEntityAlive(entity));

			EntityDestroyEvent?.Invoke(entity);

			_entityIdToGeneration[entity.id]++;
			_freeEntitiesIds[--EntitiesCount] = entity.id;
		}

		private void EnsureCapacity(int index)
		{
			if (index < EntitiesCapacity)
				return;

			do
				EntitiesCapacity += ExpandStep;
			while (index >= EntitiesCapacity);

			_freeEntitiesIds.Expand(EntitiesCapacity);
			_entityIdToGeneration.Expand(EntitiesCapacity);
#if UNITY_EDITOR
			entitiesNames.Expand(EntitiesCapacity);
#endif
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && ARCHETYPES_DEBUG)
			UnityEngine.Debug.LogWarning($"Entities Capacity was expanded to {EntitiesCapacity}");
#endif
		}
	}
}
