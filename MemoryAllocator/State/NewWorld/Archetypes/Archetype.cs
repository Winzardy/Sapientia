using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Collections;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public interface IComponent : IIndexedType
	{
	}

	public struct ArchetypeElement<TValue>
	{
		public readonly Entity entity;
		public TValue value;

		public ArchetypeElement(Entity entity, TValue value)
		{
			this.entity = entity;
			this.value = value;
		}
	}

	public unsafe interface IElementDestroyHandler<T> : IElementDestroyHandler where T: unmanaged
	{
		public void EntityDestroyed(Allocator* allocator, ref ArchetypeElement<T> elementPtr);
		public void EntityArrayDestroyed(Allocator* allocator, ArchetypeElement<T>* elementsPtr, uint count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IElementDestroyHandler.EntityDestroyed(Allocator* allocator, void* elementPtr)
		{
			EntityDestroyed(allocator, ref UnsafeExt.AsRef<ArchetypeElement<T>>(elementPtr));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IElementDestroyHandler.EntityArrayDestroyed(Allocator* allocator, void* elementsPtr, int count)
		{
			EntityArrayDestroyed(allocator, (ArchetypeElement<T>*)elementsPtr, count);
		}
	}

	[InterfaceProxy]
	public unsafe interface IElementDestroyHandler
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityDestroyed(Allocator* allocator, void* element);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityArrayDestroyed(Allocator* allocator, void* element, int count);
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Archetype : IEntityDestroySubscriber, IEnumerable<IntPtr>
	{
		public static class DefaultValue<TValue>
		{
			public static readonly TValue DEFAULT = default;
		}

		internal SparseSet _elements;

		private bool _hasDestroyHandler;
		private IElementDestroyHandlerProxy _destroyHandlerProxy;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _elements.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(Allocator* allocator, int elementsCount) where T: unmanaged, IComponent
		{
			return ref RegisterArchetype<T>(allocator, elementsCount, allocator->serviceLocator.GetService<EntitiesStatePart>().EntitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype RegisterArchetype<T>(Allocator* allocator, int elementsCount, int entitiesCapacity) where T: unmanaged, IComponent
		{
			var archetypePtr = RegisterArchetype(allocator, TSize<ArchetypeElement<T>>.size, elementsCount, entitiesCapacity);
			allocator->serviceLocator.RegisterServiceAs<Archetype, T>(archetypePtr);

			return ref archetypePtr.GetValue();
		}

		private static Ptr<Archetype> RegisterArchetype(Allocator* allocator, int size, int elementsCount, int entitiesCapacity)
		{
			var archetypePtr = Ptr<Archetype>.Create(allocator);
			ref var archetype = ref archetypePtr.GetValue(allocator);

			archetype._elements  = new SparseSet(allocator, size, elementsCount, entitiesCapacity);
			archetype._hasDestroyHandler = false;
			archetype._destroyHandlerProxy = default;

			allocator->serviceLocator.GetService<EntitiesStatePart>().AddSubscriber((ValueRef)archetypePtr);

			return archetypePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDestroyHandler<THandler>() where THandler : unmanaged, IElementDestroyHandler
		{
			_destroyHandlerProxy = IndexedTypes.GetProxy<THandler, IElementDestroyHandlerProxy>();
			_hasDestroyHandler = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ArchetypeElement<T>* GetRawElements<T>() where T: unmanaged, IComponent
		{
			return _elements.GetValuePtr<ArchetypeElement<T>>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ArchetypeElement<T>* GetRawElements<T>(Allocator* allocator) where T: unmanaged, IComponent
		{
			return _elements.GetValuePtr<ArchetypeElement<T>>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement<T>(Entity entity) where T : unmanaged, IComponent
		{
			if (!_elements.Has(entity.id))
				return ref DefaultValue<T>.DEFAULT;
			return ref _elements.Get<ArchetypeElement<T>>(entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<T>(IEnumerable<Entity> entities) where T : unmanaged, IComponent
		{
			return ReadElements<T, IEnumerable<Entity>>(entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<T, TEnumerable>(TEnumerable entities) where T : unmanaged, IComponent where TEnumerable: IEnumerable<Entity>
		{
			var result = new SimpleList<T>();
			foreach (var entity in entities)
			{
				if (_elements.Has(entity.id))
					result.Add(_elements.Get<ArchetypeElement<T>>(entity.id).value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return _elements.Has(entity.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement<T>(Allocator* allocator, Entity entity) where T : unmanaged, IComponent
		{
			if (_elements.Has(allocator, entity.id))
			{
				ref var element = ref _elements.Get<ArchetypeElement<T>>(allocator, entity.id);
				Debug.Assert(element.entity == entity);
				return ref element.value;
			}
			else
			{
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				var oldCapacity = _elements.Capacity;
				ref var element = ref _elements.EnsureGet<ArchetypeElement<T>>(allocator, entity.id);
				if (oldCapacity != _elements.Capacity)
					UnityEngine.Debug.LogWarning($"Archetype of {typeof(T).Name} was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#else
				ref var element = ref _elements.EnsureGet<ArchetypeElement<T>>(allocator, entity.id);
#endif
				element = new ArchetypeElement<T>(entity, default);
				return ref element.value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement<T>(Entity entity) where T : unmanaged, IComponent
		{
			if (_elements.Has(entity.id))
			{
				ref var element = ref _elements.Get<ArchetypeElement<T>>(entity.id);
				Debug.Assert(element.entity == entity);
				return ref element.value;
			}
			else
			{
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && DEBUG)
				var oldCapacity = _elements.Capacity;
				ref var element = ref _elements.EnsureGet<ArchetypeElement<T>>(entity.id);
				if (oldCapacity != _elements.Capacity)
					UnityEngine.Debug.LogWarning($"Archetype of {typeof(T).Name} was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#else
				ref var element = ref _elements.EnsureGet<ArchetypeElement<T>>(entity.id);
#endif
				element = new ArchetypeElement<T>(entity, default);
				return ref element.value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear<T>() where T: unmanaged, IComponent
		{
			var allocator = _elements.GetAllocatorPtr();
			if (_hasDestroyHandler)
			{
				var valueArray = _elements.GetValuePtr<ArchetypeElement<T>>(allocator);
				_destroyHandlerProxy.EntityArrayDestroyed(default, allocator, valueArray, _elements.Count);
			}
			_elements.Clear(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast<T>() where T: unmanaged, IComponent
		{
			var allocator = _elements.GetAllocatorPtr();
			if (_hasDestroyHandler)
			{
				var valueArray = _elements.GetValuePtr<ArchetypeElement<T>>(allocator);
				_destroyHandlerProxy.EntityArrayDestroyed(default, allocator, valueArray, _elements.Count);
			}

			_elements.ClearFast();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBackElement(Entity entity)
		{
			_elements.RemoveSwapBack(entity.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EntityDestroyed(Allocator* allocator, in Entity entity)
		{
			if (!_elements.Has(allocator, entity.id))
				return;

			if (_hasDestroyHandler)
			{
				var value = _elements.GetValuePtr(allocator, entity.id);
				_destroyHandlerProxy.EntityDestroyed(default, allocator, value);

				if (!_elements.Has(allocator, entity.id))
					return;
			}
			_elements.RemoveSwapBack(allocator, entity.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_elements.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>() where T: unmanaged, IComponent
		{
			return _elements.GetEnumerable<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator GetPtrEnumerator()
		{
			return _elements.GetPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
		{
			return GetPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetPtrEnumerator();
		}
	}
}
