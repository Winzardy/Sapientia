using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Collections;
using Sapientia.Collections.FixedString;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
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
		public void EntityPtrArrayDestroyed(Allocator allocator, ArchetypeElement<T>** elementsPtr, int count);
		public void EntityArrayDestroyed(Allocator allocator, ArchetypeElement<T>* elementsPtr, int count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IElementDestroyHandler.EntityPtrArrayDestroyed(Allocator allocator, void** elementsPtr, int count)
		{
			EntityPtrArrayDestroyed(allocator, (ArchetypeElement<T>**)elementsPtr, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IElementDestroyHandler.EntityArrayDestroyed(Allocator allocator, void* elementsPtr, int count)
		{
			EntityArrayDestroyed(allocator, (ArchetypeElement<T>*)elementsPtr, count);
		}

	}

	public unsafe interface IElementDestroyHandler : IInterfaceProxyType
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityPtrArrayDestroyed(Allocator allocator, void** elementsPtr, int count);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityArrayDestroyed(Allocator allocator, void* elementsPtr, int count);
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
		public Allocator GetAllocator()
		{
			return _elements.GetAllocator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(Allocator allocator, int elementsCount) where T: unmanaged, IIndexedType
		{
			return ref RegisterArchetype<T>(allocator, DataAccessorContext.Create<T, Archetype>(), elementsCount, allocator.GetService<EntityStatePart>().EntitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(Allocator allocator, DataAccessorContext context, int elementsCount) where T: unmanaged
		{
			return ref RegisterArchetype<T>(allocator, context, elementsCount, allocator.GetService<EntityStatePart>().EntitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(Allocator allocator, int elementsCount, int entitiesCapacity) where T: unmanaged, IIndexedType
		{
			return ref RegisterArchetype<T>(allocator, DataAccessorContext.Create<T, Archetype>(), elementsCount, entitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(Allocator allocator, DataAccessorContext context, int elementsCount, int entitiesCapacity) where T: unmanaged
		{
			var archetypePtr = CreateArchetype<T>(allocator, elementsCount, entitiesCapacity);
			context.RegisterService(allocator, archetypePtr);

			return ref archetypePtr.GetValue();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(Allocator allocator, int elementsCount, out ArchetypePtr<T> archetypePtr) where T: unmanaged, IComponent
		{
			return ref RegisterArchetype<T>(allocator, DataAccessorContext.Create<T, Archetype>(), elementsCount, allocator.GetService<EntityStatePart>().EntitiesCapacity, out archetypePtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(Allocator allocator, DataAccessorContext context, int elementsCount, out ArchetypePtr<T> archetypePtr) where T: unmanaged, IComponent
		{
			return ref RegisterArchetype<T>(allocator, context, elementsCount, allocator.GetService<EntityStatePart>().EntitiesCapacity, out archetypePtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(Allocator allocator, int elementsCount, int entitiesCapacity, out ArchetypePtr<T> archetypePtr) where T: unmanaged, IComponent
		{
			return ref RegisterArchetype<T>(allocator, DataAccessorContext.Create<T, Archetype>(), elementsCount, entitiesCapacity, out archetypePtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(Allocator allocator, DataAccessorContext context, int elementsCount, int entitiesCapacity, out ArchetypePtr<T> archetypePtr) where T: unmanaged, IComponent
		{
			archetypePtr = CreateArchetype<T>(allocator, elementsCount, entitiesCapacity);
			context.RegisterService(allocator, archetypePtr);

			return ref archetypePtr.GetArchetype(allocator);
		}

		public static Ptr<Archetype> CreateArchetype<T>(Allocator allocator, int elementsCount, int entitiesCapacity) where T: unmanaged
		{
			var archetypePtr = Ptr<Archetype>.Create(allocator);
			ref var archetype = ref archetypePtr.GetValue(allocator);

			archetype._elements = new SparseSet(allocator, TSize<ArchetypeElement<T>>.size, elementsCount, entitiesCapacity);
			archetype._destroyHandlerProxy = default;
#if DEBUG
			archetype.elementTypeName = typeof(T).Name;
#endif

			allocator.GetService<EntityStatePart>().AddSubscriber(allocator, (IndexedPtr)archetypePtr);

			return archetypePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ptr<Archetype> RegisterArchetype(Allocator allocator, DataAccessorContext context, int elementsCount)
		{
			var ptr = CreateArchetype(allocator, elementsCount, allocator.GetService<EntityStatePart>().EntitiesCapacity);
			context.RegisterService(allocator, (Ptr)ptr);

			return ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ptr<Archetype> CreateArchetype(Allocator allocator, int elementsCount)
		{
			return CreateArchetype(allocator, elementsCount, allocator.GetService<EntityStatePart>().EntitiesCapacity);
		}

		public static Ptr<Archetype> CreateArchetype(Allocator allocator, int elementsCount, int entitiesCapacity)
		{
			var archetypePtr = Ptr<Archetype>.Create(allocator);
			ref var archetype = ref archetypePtr.GetValue(allocator);

			archetype._elements = new SparseSet(allocator, TSize<ArchetypeElement>.size, elementsCount, entitiesCapacity);
			archetype._destroyHandlerProxy = default;

			allocator.GetService<EntityStatePart>().AddSubscriber(allocator, (IndexedPtr)archetypePtr);

			return archetypePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDestroyHandler<THandler>() where THandler : unmanaged, IElementDestroyHandler
		{
			_destroyHandlerProxy = ProxyPtr<IElementDestroyHandlerProxy>.Create<THandler>(_elements.GetAllocator());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDestroyHandler<THandler>(Allocator allocator) where THandler : unmanaged, IElementDestroyHandler
		{
			_destroyHandlerProxy = ProxyPtr<IElementDestroyHandlerProxy>.Create<THandler>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<ArchetypeElement<T>> GetRawElements<T>() where T: unmanaged
		{
			return _elements.GetValuePtr<ArchetypeElement<T>>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<ArchetypeElement<T>> GetRawElements<T>(Allocator allocator) where T: unmanaged
		{
			return _elements.GetValuePtr<ArchetypeElement<T>>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryReadElement<T>(Allocator allocator, Entity entity, out T result) where T : unmanaged
		{
			if (!_elements.Has(allocator, entity.id))
			{
				result = default;
				return false;
			}
			result = _elements.Get<ArchetypeElement<T>>(allocator, entity.id).value;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement<T>(Entity entity) where T : unmanaged
		{
			return ref ReadElement<T>(entity.GetAllocator(), entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement<T>(Allocator allocator, Entity entity) where T : unmanaged
		{
			if (!_elements.Has(allocator, entity.id))
				return ref DefaultValue<T>.DEFAULT;
			return ref _elements.Get<ArchetypeElement<T>>(allocator, entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement<T>(Allocator allocator, Entity entity, out bool isExist) where T : unmanaged
		{
			isExist = _elements.Has(allocator, entity.id);
			if (!isExist)
				return ref DefaultValue<T>.DEFAULT;
			return ref _elements.Get<ArchetypeElement<T>>(allocator, entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElementNoCheck<T>(Allocator allocator, Entity entity) where T : unmanaged
		{
			return ref _elements.Get<ArchetypeElement<T>>(allocator, entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<T>(IEnumerable<Entity> entities) where T : unmanaged
		{
			return ReadElements<T, IEnumerable<Entity>>(entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<T, TEnumerable>(TEnumerable entities) where T : unmanaged where TEnumerable: IEnumerable<Entity>
		{
			return ReadElements<T, TEnumerable>(GetAllocator(), entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<T, TEnumerable>(Allocator allocator, TEnumerable entities) where T : unmanaged where TEnumerable: IEnumerable<Entity>
		{
			var result = new SimpleList<T>();
			foreach (var entity in entities)
			{
				if (_elements.Has(allocator, entity.id))
					result.Add(_elements.Get<ArchetypeElement<T>>(allocator, entity.id).value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return HasElement(entity.GetAllocator(), entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Allocator allocator, Entity entity)
		{
			return _elements.Has(allocator, entity.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetElement(Entity entity)
		{
			return SetElement(_elements.GetAllocator(), entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetElement(Allocator allocator, Entity entity)
		{
			if (_elements.Has(allocator, entity.id))
				return false;

#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
			var oldCapacity = _elements.Capacity;
#endif
			_elements.EnsureGet<ArchetypeElement>(allocator, entity.id);
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
			if (oldCapacity != _elements.Capacity)
				UnityEngine.Debug.LogWarning($"Archetype was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#endif
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetElement<T>(Allocator allocator, Entity entity, out bool isExist) where T : unmanaged
		{
			if (_elements.Has(allocator, entity.id))
			{
				ref var element = ref _elements.Get<ArchetypeElement<T>>(allocator, entity.id);
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
		public ref T GetElement<T>(Allocator allocator, Entity entity, out bool isCreated) where T : unmanaged
		{
			if (_elements.Has(allocator, entity.id))
			{
				ref var element = ref _elements.Get<ArchetypeElement<T>>(allocator, entity.id);
				E.ASSERT(element.entity == entity);

				isCreated = false;
				return ref element.value;
			}
			else
			{
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				var oldCapacity = _elements.Capacity;
#endif
				ref var element = ref _elements.EnsureGet<ArchetypeElement<T>>(allocator, entity.id);
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				if (oldCapacity != _elements.Capacity)
					UnityEngine.Debug.LogWarning($"Archetype of {typeof(T).Name} was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#endif
				element = new ArchetypeElement<T>(entity, default);

				isCreated = true;
				return ref element.value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement<T>(Entity entity, out bool isCreated) where T : unmanaged
		{
			return ref GetElement<T>(_elements.GetAllocator(), entity, out isCreated);
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
		public ref T GetElement<T>(Allocator allocator, Entity entity) where T : unmanaged
		{
			if (_elements.Has(allocator, entity.id))
			{
				ref var element = ref _elements.Get<ArchetypeElement<T>>(allocator, entity.id);
				E.ASSERT(element.entity == entity);

				return ref element.value;
			}
			else
			{
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				var oldCapacity = _elements.Capacity;
#endif
				ref var element = ref _elements.EnsureGet<ArchetypeElement<T>>(allocator, entity.id);
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				if (oldCapacity != _elements.Capacity)
					UnityEngine.Debug.LogWarning($"Archetype of {typeof(T).Name} was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#endif
				element = new ArchetypeElement<T>(entity, default);

				return ref element.value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement<T>(Entity entity) where T : unmanaged
		{
			return ref GetElement<T>(_elements.GetAllocator(), entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear<T>() where T: unmanaged
		{
			var allocator = _elements.GetAllocator();
			if (_destroyHandlerProxy.IsCreated)
			{
				var valueArray = _elements.GetValuePtr<ArchetypeElement<T>>(allocator);
				_destroyHandlerProxy.EntityArrayDestroyed(allocator, allocator, valueArray.ptr, _elements.Count);
			}
			_elements.Clear(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast<T>() where T: unmanaged
		{
			var allocator = _elements.GetAllocator();
			if (_destroyHandlerProxy.IsCreated)
			{
				var valueArray = _elements.GetValuePtr<ArchetypeElement<T>>(allocator);
				_destroyHandlerProxy.EntityArrayDestroyed(allocator, allocator, valueArray.ptr, _elements.Count);
			}

			_elements.ClearFast();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBackElement(Entity entity)
		{
			RemoveSwapBackElement(_elements.GetAllocator(), entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBackElement(Allocator allocator, Entity entity)
		{
			if (_elements.TryGetDenseId(allocator, entity.id, out var denseId))
			{
				if (_destroyHandlerProxy.IsCreated)
				{
					_destroyHandlerProxy.EntityArrayDestroyed(allocator, allocator, _elements.GetValuePtrByDenseId(allocator, entity.id).ptr, 1);
				}
				_elements.RemoveSwapBackByDenseId(allocator, denseId);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityArrayDestroyed(Allocator allocator, Entity* entities, int count)
		{
			if (_destroyHandlerProxy.IsCreated)
			{
				var archetypeEntities = stackalloc void*[_elements.Count.Min(count)];
				var archetypeEntitiesCount = 0;

				for (var i = 0; i < count; i++)
				{
					var entityId = entities[i].id;
					if (!_elements.Has(allocator, entityId))
						continue;
					archetypeEntities[archetypeEntitiesCount++] = _elements.GetValuePtr(allocator, entityId).ptr;
				}

				_destroyHandlerProxy.EntityPtrArrayDestroyed(allocator, allocator, archetypeEntities, archetypeEntitiesCount);
			}

			for (var i = 0; i < count; i++)
			{
				_elements.RemoveSwapBack(allocator, entities[i].id);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_elements.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(Allocator allocator)
		{
			_elements.Dispose(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>() where T: unmanaged
		{
			return _elements.GetEnumerable<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator<T> GetPtrEnumerator<T>() where T: unmanaged
		{
			return _elements.GetPtrEnumerator<T>();
		}
	}
}
