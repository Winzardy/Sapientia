using System;
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
	/// Доступ к настройкам в рантайме осуществляется через
	/// <see cref="allocator.GetService{IConfigurationRuntime}"/>, если они заданы.
	/// </summary>
	public interface IConfigurationRuntime : IIndexedType
	{
	}

	public interface IComponent : IIndexedType
	{
	}

	public struct ComponentSetElement<TValue>
	{
		public Entity entity;
		public TValue value;

		public ComponentSetElement(Entity entity, TValue value)
		{
			this.entity = entity;
			this.value = value;
		}
	}

	public struct ComponentSetElement
	{
		public readonly Entity entity;

		public ComponentSetElement(Entity entity)
		{
			this.entity = entity;
		}
	}

	public unsafe interface IElementDestroyHandler<T> : IElementDestroyHandler where T: unmanaged, IComponent
	{
		public void EntityPtrArrayDestroyed(WorldState worldState, ComponentSetElement<T>** elementsPtr, int count);
		public void EntityArrayDestroyed(WorldState worldState, ComponentSetElement<T>* elementsPtr, int count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IElementDestroyHandler.EntityPtrArrayDestroyed(WorldState worldState, void** elementsPtr, int count)
		{
			EntityPtrArrayDestroyed(worldState, (ComponentSetElement<T>**)elementsPtr, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IElementDestroyHandler.EntityArrayDestroyed(WorldState worldState, void* elementsPtr, int count)
		{
			EntityArrayDestroyed(worldState, (ComponentSetElement<T>*)elementsPtr, count);
		}
	}

	public unsafe interface IElementDestroyHandler : IInterfaceProxyType
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityPtrArrayDestroyed(WorldState worldState, void** elementsPtr, int count);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityArrayDestroyed(WorldState worldState, void* elementsPtr, int count);
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct ComponentSet : IEntityDestroySubscriber, IIndexedType
	{
		internal MemSparseSet _elements;

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
		public static ref ComponentSet RegisterComponentSet<T>(WorldState worldState, int elementsCount) where T: unmanaged, IIndexedType
		{
			return ref RegisterComponentSet<T>(worldState, ServiceRegistryContext.Create<T, ComponentSet>(), elementsCount, worldState.GetService<EntityStatePart>().EntitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref ComponentSet RegisterComponentSet<T>(WorldState worldState, ServiceRegistryContext context, int elementsCount) where T: unmanaged
		{
			return ref RegisterComponentSet<T>(worldState, context, elementsCount, worldState.GetService<EntityStatePart>().EntitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref ComponentSet RegisterComponentSet<T>(WorldState worldState, int elementsCount, int entitiesCapacity) where T: unmanaged, IIndexedType
		{
			return ref RegisterComponentSet<T>(worldState, ServiceRegistryContext.Create<T, ComponentSet>(), elementsCount, entitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref ComponentSet RegisterComponentSet<T>(WorldState worldState, ServiceRegistryContext context, int elementsCount, int entitiesCapacity) where T: unmanaged
		{
			var componentSetPtr = CreateComponentSet<T>(worldState, elementsCount, entitiesCapacity);
			context.RegisterService(worldState, componentSetPtr);

			return ref componentSetPtr.GetValue(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref ComponentSet RegisterComponentSet<T>(WorldState worldState, int elementsCount, out ComponentSetPtr<T> componentSetPtr) where T: unmanaged, IComponent
		{
			return ref RegisterComponentSet<T>(worldState, ServiceRegistryContext.Create<T, ComponentSet>(), elementsCount, worldState.GetService<EntityStatePart>().EntitiesCapacity, out componentSetPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref ComponentSet RegisterComponentSet<T>(WorldState worldState, ServiceRegistryContext context, int elementsCount, out ComponentSetPtr<T> componentSetPtr) where T: unmanaged, IComponent
		{
			return ref RegisterComponentSet<T>(worldState, context, elementsCount, worldState.GetService<EntityStatePart>().EntitiesCapacity, out componentSetPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref ComponentSet RegisterComponentSet<T>(WorldState worldState, int elementsCount, int entitiesCapacity, out ComponentSetPtr<T> componentSetPtr) where T: unmanaged, IComponent
		{
			return ref RegisterComponentSet<T>(worldState, ServiceRegistryContext.Create<T, ComponentSet>(), elementsCount, entitiesCapacity, out componentSetPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref ComponentSet RegisterComponentSet<T>(WorldState worldState, ServiceRegistryContext context, int elementsCount, int entitiesCapacity, out ComponentSetPtr<T> componentSetPtr) where T: unmanaged, IComponent
		{
			componentSetPtr = CreateComponentSet<T>(worldState, elementsCount, entitiesCapacity);
			context.RegisterService(worldState, componentSetPtr);

			return ref componentSetPtr.GetComponentSet(worldState);
		}

		public static CachedPtr<ComponentSet> CreateComponentSet<T>(WorldState worldState, int elementsCount, int entitiesCapacity) where T: unmanaged
		{
			var componentSetPtr = CachedPtr<ComponentSet>.Create(worldState);
			ref var componentSet = ref componentSetPtr.GetValue(worldState);

			componentSet._elements = new MemSparseSet(worldState, TSize<ComponentSetElement<T>>.size, elementsCount, entitiesCapacity);
			componentSet._destroyHandlerProxy = default;
#if DEBUG
			componentSet.elementTypeName = typeof(T).Name;
#endif

			worldState.GetService<EntityStatePart>().AddSubscriber(worldState, (IndexedPtr)componentSetPtr);

			return componentSetPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CachedPtr<ComponentSet> RegisterComponentSet(WorldState worldState, ServiceRegistryContext context, int elementsCount)
		{
			var ptr = CreateComponentSet(worldState, elementsCount, worldState.GetService<EntityStatePart>().EntitiesCapacity);
			context.RegisterService(worldState, (CachedPtr)ptr);

			return ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CachedPtr<ComponentSet> CreateComponentSet(WorldState worldState, int elementsCount)
		{
			return CreateComponentSet(worldState, elementsCount, worldState.GetService<EntityStatePart>().EntitiesCapacity);
		}

		public static CachedPtr<ComponentSet> CreateComponentSet(WorldState worldState, int elementsCount, int entitiesCapacity)
		{
			var componentSetPtr = CachedPtr<ComponentSet>.Create(worldState);
			ref var componentSet = ref componentSetPtr.GetValue(worldState);

			componentSet._elements = new MemSparseSet(worldState, TSize<ComponentSetElement>.size, elementsCount, entitiesCapacity);
			componentSet._destroyHandlerProxy = default;

			worldState.GetService<EntityStatePart>().AddSubscriber(worldState, (IndexedPtr)componentSetPtr);

			return componentSetPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDestroyHandler<THandler>(WorldState worldState) where THandler : unmanaged, IElementDestroyHandler
		{
			_destroyHandlerProxy = ProxyPtr<IElementDestroyHandlerProxy>.Create<THandler>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<ComponentSetElement<T>> GetRawElements<T>(WorldState worldState) where T: unmanaged
		{
			return _elements.GetValuePtr<ComponentSetElement<T>>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<ComponentSetElement<T>> GetSpan<T>(WorldState worldState) where T: unmanaged
		{
			return _elements.GetSpan<ComponentSetElement<T>>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryReadElement<T>(WorldState worldState, Entity entity, out T result) where T : unmanaged
		{
			if (!_elements.Has(worldState, entity.id))
			{
				result = default;
				return false;
			}
			result = _elements.Get<ComponentSetElement<T>>(worldState, entity.id).value;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement<T>(Entity entity) where T : unmanaged
		{
			return ref ReadElement<T>(entity.GetWorldState(), entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement<T>(WorldState worldState, Entity entity) where T : unmanaged
		{
			if (!_elements.Has(worldState, entity.id))
				return ref TReadonlyDefaultValue<T>.value;
			return ref _elements.Get<ComponentSetElement<T>>(worldState, entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement<T>(WorldState worldState, Entity entity, out bool isExist) where T : unmanaged
		{
			isExist = _elements.Has(worldState, entity.id);
			if (!isExist)
				return ref TReadonlyDefaultValue<T>.value;
			return ref _elements.Get<ComponentSetElement<T>>(worldState, entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElementNoCheck<T>(WorldState worldState, Entity entity) where T : unmanaged
		{
			return ref _elements.Get<ComponentSetElement<T>>(worldState, entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<T>(WorldState worldState, IEnumerable<Entity> entities) where T : unmanaged
		{
			return ReadElements<T, IEnumerable<Entity>>(worldState, entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<T, TEnumerable>(WorldState worldState, TEnumerable entities) where T : unmanaged where TEnumerable: IEnumerable<Entity>
		{
			var result = new SimpleList<T>();
			foreach (var entity in entities)
			{
				if (_elements.Has(worldState, entity.id))
					result.Add(_elements.Get<ComponentSetElement<T>>(worldState, entity.id).value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return HasElement(entity.GetWorldState(), entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(WorldState worldState, Entity entity)
		{
			return _elements.Has(worldState, entity.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetElement(WorldState worldState, Entity entity)
		{
			if (_elements.Has(worldState, entity.id))
				return false;

#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
			var oldCapacity = _elements.Capacity;
#endif
			_elements.EnsureGet<ComponentSetElement>(worldState, entity.id);
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
			if (oldCapacity != _elements.Capacity)
				UnityEngine.Debug.LogWarning($"ComponentSet was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#endif
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetElement<T>(WorldState worldState, Entity entity, out bool isExist) where T : unmanaged
		{
			if (_elements.Has(worldState, entity.id))
			{
				ref var element = ref _elements.Get<ComponentSetElement<T>>(worldState, entity.id);
				if (element.entity.generation == entity.generation)
				{
					E.ASSERT(element.entity == entity);

					isExist = true;
					return ref element.value;
				}
			}
			isExist = false;
			return ref _elements.GetValuePtr<T>(worldState)[0];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement<T>(WorldState worldState, Entity entity, out bool isExist) where T : unmanaged
		{
			if (_elements.Has(worldState, entity.id))
			{
				ref var element = ref _elements.Get<ComponentSetElement<T>>(worldState, entity.id);
				E.ASSERT(element.entity == entity);

				isExist = true;
				return ref element.value;
			}
			else
			{
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				var oldCapacity = _elements.Capacity;
#endif
				ref var element = ref _elements.EnsureGet<ComponentSetElement<T>>(worldState, entity.id);
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				if (oldCapacity != _elements.Capacity)
					UnityEngine.Debug.LogWarning($"ComponentSet of {typeof(T).Name} was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#endif
				element = new ComponentSetElement<T>(entity, default);

				isExist = false;
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
		public ref T GetElement<T>(WorldState worldState, Entity entity) where T : unmanaged
		{
			if (_elements.Has(worldState, entity.id))
			{
				ref var element = ref _elements.Get<ComponentSetElement<T>>(worldState, entity.id);
				E.ASSERT(element.entity == entity);

				return ref element.value;
			}
			else
			{
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				var oldCapacity = _elements.Capacity;
#endif
				ref var element = ref _elements.EnsureGet<ComponentSetElement<T>>(worldState, entity.id);
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				if (oldCapacity != _elements.Capacity)
					UnityEngine.Debug.LogWarning($"ComponentSet of {typeof(T).Name} was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#endif
				element = new ComponentSetElement<T>(entity, default);

				return ref element.value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear<T>(WorldState worldState) where T: unmanaged
		{
			if (_destroyHandlerProxy.IsCreated)
			{
				var valueArray = _elements.GetValuePtr<ComponentSetElement<T>>(worldState);
				_destroyHandlerProxy.EntityArrayDestroyed(worldState, worldState, valueArray.ptr, _elements.Count);
			}
			_elements.Clear(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast<T>(WorldState worldState) where T: unmanaged
		{
			if (_destroyHandlerProxy.IsCreated)
			{
				var valueArray = _elements.GetValuePtr<ComponentSetElement<T>>(worldState);
				_destroyHandlerProxy.EntityArrayDestroyed(worldState, worldState, valueArray.ptr, _elements.Count);
			}
			_elements.ClearFast();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBackElement(WorldState worldState, Entity entity)
		{
			if (_elements.TryGetDenseId(worldState, entity.id, out var denseId))
			{
				if (_destroyHandlerProxy.IsCreated)
				{
					var safePtr = _elements.GetValuePtrByDenseId(worldState, denseId);
					_destroyHandlerProxy.EntityArrayDestroyed(worldState, worldState, safePtr.ptr, 1);
				}
				_elements.RemoveSwapBackByDenseId(worldState, denseId);

				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityArrayDestroyed(WorldState worldState, Entity* entities, int count)
		{
			if (_destroyHandlerProxy.IsCreated)
			{
				var componentSetEntities = stackalloc void*[_elements.Count.Min(count)];
				var componentSetEntitiesCount = 0;

				for (var i = 0; i < count; i++)
				{
					var entityId = entities[i].id;
					if (!_elements.Has(worldState, entityId))
						continue;
					componentSetEntities[componentSetEntitiesCount++] = _elements.GetValuePtr(worldState, entityId).ptr;
				}

				_destroyHandlerProxy.EntityPtrArrayDestroyed(worldState, worldState, componentSetEntities, componentSetEntitiesCount);
			}

			for (var i = 0; i < count; i++)
			{
				_elements.RemoveSwapBack(worldState, entities[i].id);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			_elements.Dispose(worldState);
		}
	}
}
