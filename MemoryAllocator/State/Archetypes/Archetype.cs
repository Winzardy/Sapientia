using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Collections;
using Sapientia.Collections.FixedString;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Если настройки есть, то они будут доступны через `allocator.GetService<TGlobalSettings>()`
	/// </summary>
	public interface IConfiguration : IIndexedType
	{
	}

	public interface IComponent : IIndexedType
	{
	}

	public struct ArchetypeElement<TValue>
	{
		public Entity entity;
		public TValue value;

		public ArchetypeElement(Entity entity, TValue value)
		{
			this.entity = entity;
			this.value = value;
		}
	}

	public struct ArchetypeElement
	{
		public readonly Entity entity;

		public ArchetypeElement(Entity entity)
		{
			this.entity = entity;
		}
	}

	public unsafe interface IElementDestroyHandler<T> : IElementDestroyHandler where T: unmanaged, IComponent
	{
		public void EntityPtrArrayDestroyed(World world, ArchetypeElement<T>** elementsPtr, int count);
		public void EntityArrayDestroyed(World world, ArchetypeElement<T>* elementsPtr, int count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IElementDestroyHandler.EntityPtrArrayDestroyed(World world, void** elementsPtr, int count)
		{
			EntityPtrArrayDestroyed(world, (ArchetypeElement<T>**)elementsPtr, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IElementDestroyHandler.EntityArrayDestroyed(World world, void* elementsPtr, int count)
		{
			EntityArrayDestroyed(world, (ArchetypeElement<T>*)elementsPtr, count);
		}

	}

	public unsafe interface IElementDestroyHandler : IInterfaceProxyType
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityPtrArrayDestroyed(World world, void** elementsPtr, int count);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityArrayDestroyed(World world, void* elementsPtr, int count);
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Archetype : IEntityDestroySubscriber, IIndexedType
	{
		public static class DefaultValue<TValue>
		{
			public static readonly TValue DEFAULT = default;
		}

		internal SparseSet _elements;

		private ProxyPtr<IElementDestroyHandlerProxy> _destroyHandlerProxy;

#if DEBUG
		public FixedString64Bytes elementTypeName;
#endif

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _elements.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(World world, int elementsCount) where T: unmanaged, IIndexedType
		{
			return ref RegisterArchetype<T>(world, ServiceRegistryContext.Create<T, Archetype>(), elementsCount, world.GetService<EntityStatePart>().EntitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(World world, ServiceRegistryContext context, int elementsCount) where T: unmanaged
		{
			return ref RegisterArchetype<T>(world, context, elementsCount, world.GetService<EntityStatePart>().EntitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(World world, int elementsCount, int entitiesCapacity) where T: unmanaged, IIndexedType
		{
			return ref RegisterArchetype<T>(world, ServiceRegistryContext.Create<T, Archetype>(), elementsCount, entitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(World world, ServiceRegistryContext context, int elementsCount, int entitiesCapacity) where T: unmanaged
		{
			var archetypePtr = CreateArchetype<T>(world, elementsCount, entitiesCapacity);
			context.RegisterService(world, archetypePtr);

			return ref archetypePtr.GetValue(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(World world, int elementsCount, out ArchetypePtr<T> archetypePtr) where T: unmanaged, IComponent
		{
			return ref RegisterArchetype<T>(world, ServiceRegistryContext.Create<T, Archetype>(), elementsCount, world.GetService<EntityStatePart>().EntitiesCapacity, out archetypePtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(World world, ServiceRegistryContext context, int elementsCount, out ArchetypePtr<T> archetypePtr) where T: unmanaged, IComponent
		{
			return ref RegisterArchetype<T>(world, context, elementsCount, world.GetService<EntityStatePart>().EntitiesCapacity, out archetypePtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(World world, int elementsCount, int entitiesCapacity, out ArchetypePtr<T> archetypePtr) where T: unmanaged, IComponent
		{
			return ref RegisterArchetype<T>(world, ServiceRegistryContext.Create<T, Archetype>(), elementsCount, entitiesCapacity, out archetypePtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(World world, ServiceRegistryContext context, int elementsCount, int entitiesCapacity, out ArchetypePtr<T> archetypePtr) where T: unmanaged, IComponent
		{
			archetypePtr = CreateArchetype<T>(world, elementsCount, entitiesCapacity);
			context.RegisterService(world, archetypePtr);

			return ref archetypePtr.GetArchetype(world);
		}

		public static CachedPtr<Archetype> CreateArchetype<T>(World world, int elementsCount, int entitiesCapacity) where T: unmanaged
		{
			var archetypePtr = CachedPtr<Archetype>.Create(world);
			ref var archetype = ref archetypePtr.GetValue(world);

			archetype._elements = new SparseSet(world, TSize<ArchetypeElement<T>>.size, elementsCount, entitiesCapacity);
			archetype._destroyHandlerProxy = default;
#if DEBUG
			archetype.elementTypeName = typeof(T).Name;
#endif

			world.GetService<EntityStatePart>().AddSubscriber(world, (IndexedPtr)archetypePtr);

			return archetypePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CachedPtr<Archetype> RegisterArchetype(World world, ServiceRegistryContext context, int elementsCount)
		{
			var ptr = CreateArchetype(world, elementsCount, world.GetService<EntityStatePart>().EntitiesCapacity);
			context.RegisterService(world, (CachedPtr)ptr);

			return ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CachedPtr<Archetype> CreateArchetype(World world, int elementsCount)
		{
			return CreateArchetype(world, elementsCount, world.GetService<EntityStatePart>().EntitiesCapacity);
		}

		public static CachedPtr<Archetype> CreateArchetype(World world, int elementsCount, int entitiesCapacity)
		{
			var archetypePtr = CachedPtr<Archetype>.Create(world);
			ref var archetype = ref archetypePtr.GetValue(world);

			archetype._elements = new SparseSet(world, TSize<ArchetypeElement>.size, elementsCount, entitiesCapacity);
			archetype._destroyHandlerProxy = default;

			world.GetService<EntityStatePart>().AddSubscriber(world, (IndexedPtr)archetypePtr);

			return archetypePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDestroyHandler<THandler>(World world) where THandler : unmanaged, IElementDestroyHandler
		{
			_destroyHandlerProxy = ProxyPtr<IElementDestroyHandlerProxy>.Create<THandler>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<ArchetypeElement<T>> GetRawElements<T>(World world) where T: unmanaged
		{
			return _elements.GetValuePtr<ArchetypeElement<T>>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryReadElement<T>(World world, Entity entity, out T result) where T : unmanaged
		{
			if (!_elements.Has(world, entity.id))
			{
				result = default;
				return false;
			}
			result = _elements.Get<ArchetypeElement<T>>(world, entity.id).value;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement<T>(Entity entity) where T : unmanaged
		{
			return ref ReadElement<T>(entity.GetAllocator(), entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement<T>(World world, Entity entity) where T : unmanaged
		{
			if (!_elements.Has(world, entity.id))
				return ref DefaultValue<T>.DEFAULT;
			return ref _elements.Get<ArchetypeElement<T>>(world, entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement<T>(World world, Entity entity, out bool isExist) where T : unmanaged
		{
			isExist = _elements.Has(world, entity.id);
			if (!isExist)
				return ref DefaultValue<T>.DEFAULT;
			return ref _elements.Get<ArchetypeElement<T>>(world, entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElementNoCheck<T>(World world, Entity entity) where T : unmanaged
		{
			return ref _elements.Get<ArchetypeElement<T>>(world, entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<T>(World world, IEnumerable<Entity> entities) where T : unmanaged
		{
			return ReadElements<T, IEnumerable<Entity>>(world, entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<T, TEnumerable>(World world, TEnumerable entities) where T : unmanaged where TEnumerable: IEnumerable<Entity>
		{
			var result = new SimpleList<T>();
			foreach (var entity in entities)
			{
				if (_elements.Has(world, entity.id))
					result.Add(_elements.Get<ArchetypeElement<T>>(world, entity.id).value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return HasElement(entity.GetAllocator(), entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(World world, Entity entity)
		{
			return _elements.Has(world, entity.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetElement(World world, Entity entity)
		{
			if (_elements.Has(world, entity.id))
				return false;

#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
			var oldCapacity = _elements.Capacity;
#endif
			_elements.EnsureGet<ArchetypeElement>(world, entity.id);
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
			if (oldCapacity != _elements.Capacity)
				UnityEngine.Debug.LogWarning($"Archetype was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#endif
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetElement<T>(World world, Entity entity, out bool isExist) where T : unmanaged
		{
			if (_elements.Has(world, entity.id))
			{
				ref var element = ref _elements.Get<ArchetypeElement<T>>(world, entity.id);
				if (element.entity.generation == entity.generation)
				{
					E.ASSERT(element.entity == entity);

					isExist = true;
					return ref element.value;
				}
			}
			isExist = false;
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement<T>(World world, Entity entity, out bool isCreated) where T : unmanaged
		{
			if (_elements.Has(world, entity.id))
			{
				ref var element = ref _elements.Get<ArchetypeElement<T>>(world, entity.id);
				E.ASSERT(element.entity == entity);

				isCreated = false;
				return ref element.value;
			}
			else
			{
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				var oldCapacity = _elements.Capacity;
#endif
				ref var element = ref _elements.EnsureGet<ArchetypeElement<T>>(world, entity.id);
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				if (oldCapacity != _elements.Capacity)
					UnityEngine.Debug.LogWarning($"Archetype of {typeof(T).Name} was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#endif
				element = new ArchetypeElement<T>(entity, default);

				isCreated = true;
				return ref element.value;
			}
		}

		/// Тут идёт дублирование кода, т.к:
		/// 1) Если написать:
		///		return ref GetElement<T>(_elements.GetAllocator(), entity, out _);
		/// Будет ошибка: "An expression cannot be used in this context because it cannot be passed or returned by reference"
		/// 2) Если написать:
		///		bool b;
		///		return ref GetElement<T>(_elements.GetAllocator(), entity, out b);
		///	Будет предупреждение: "This returns local variable '_' by reference but it is not a ref local"
		/// Которое Unity считает ошибкой компиляции.
		///
		/// В общем это тупо, но обойти не получается...
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement<T>(World world, Entity entity) where T : unmanaged
		{
			if (_elements.Has(world, entity.id))
			{
				ref var element = ref _elements.Get<ArchetypeElement<T>>(world, entity.id);
				E.ASSERT(element.entity == entity);

				return ref element.value;
			}
			else
			{
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				var oldCapacity = _elements.Capacity;
#endif
				ref var element = ref _elements.EnsureGet<ArchetypeElement<T>>(world, entity.id);
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				if (oldCapacity != _elements.Capacity)
					UnityEngine.Debug.LogWarning($"Archetype of {typeof(T).Name} was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#endif
				element = new ArchetypeElement<T>(entity, default);

				return ref element.value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear<T>(World world) where T: unmanaged
		{
			if (_destroyHandlerProxy.IsCreated)
			{
				var valueArray = _elements.GetValuePtr<ArchetypeElement<T>>(world);
				_destroyHandlerProxy.EntityArrayDestroyed(world, world, valueArray.ptr, _elements.Count);
			}
			_elements.Clear(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast<T>(World world) where T: unmanaged
		{
			if (_destroyHandlerProxy.IsCreated)
			{
				var valueArray = _elements.GetValuePtr<ArchetypeElement<T>>(world);
				_destroyHandlerProxy.EntityArrayDestroyed(world, world, valueArray.ptr, _elements.Count);
			}
			_elements.ClearFast();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBackElement(World world, Entity entity)
		{
			if (_elements.TryGetDenseId(world, entity.id, out var denseId))
			{
				if (_destroyHandlerProxy.IsCreated)
				{
					_destroyHandlerProxy.EntityArrayDestroyed(world, world, _elements.GetValuePtrByDenseId(world, entity.id).ptr, 1);
				}
				_elements.RemoveSwapBackByDenseId(world, denseId);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityArrayDestroyed(World world, Entity* entities, int count)
		{
			if (_destroyHandlerProxy.IsCreated)
			{
				var archetypeEntities = stackalloc void*[_elements.Count.Min(count)];
				var archetypeEntitiesCount = 0;

				for (var i = 0; i < count; i++)
				{
					var entityId = entities[i].id;
					if (!_elements.Has(world, entityId))
						continue;
					archetypeEntities[archetypeEntitiesCount++] = _elements.GetValuePtr(world, entityId).ptr;
				}

				_destroyHandlerProxy.EntityPtrArrayDestroyed(world, world, archetypeEntities, archetypeEntitiesCount);
			}

			for (var i = 0; i < count; i++)
			{
				_elements.RemoveSwapBack(world, entities[i].id);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(World world)
		{
			_elements.Dispose(world);
		}
	}
}
