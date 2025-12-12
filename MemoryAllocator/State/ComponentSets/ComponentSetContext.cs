using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator.State
{
	public readonly unsafe struct ComponentSetContext<T> where T: unmanaged, IComponent
	{
		private readonly WorldState _worldState;
		private readonly SafePtr<ComponentSet> _innerArchetype;

		private WorldState WorldState
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				E.ASSERT(_worldState.IsValid, "Возможно, ComponentSetContext не был создан. Убедитесь, что вы его создали через конструктор.");
				return _worldState;
			}
		}

		public ComponentSetContext(WorldState worldState) : this(worldState, worldState.GetComponentSetPtr<T>())
		{
		}

		public ComponentSetContext(WorldState worldState, SafePtr<ComponentSet> innerArchetype)
		{
			_worldState = worldState;
			_innerArchetype = innerArchetype;
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerArchetype.ptr->Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDestroyHandler<THandler>() where THandler : unmanaged, IElementDestroyHandler
		{
			_innerArchetype.ptr->SetDestroyHandler<THandler>(WorldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<ComponentSetElement<T>> GetRawElements()
		{
			return _innerArchetype.ptr->GetRawElements<T>(WorldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<ComponentSetElement<T>> GetSpan()
		{
			return _innerArchetype.ptr->GetSpan<T>(WorldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity)
		{
			return ref _innerArchetype.ptr->ReadElement<T>(WorldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity, out bool isExist)
		{
			return ref _innerArchetype.ptr->ReadElement<T>(WorldState, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElementNoCheck(Entity entity)
		{
			return ref _innerArchetype.ptr->ReadElementNoCheck<T>(WorldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<TEnumerable>(in TEnumerable entities) where TEnumerable: IEnumerable<Entity>
		{
			return _innerArchetype.ptr->ReadElements<T, TEnumerable>(WorldState, entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return _innerArchetype.ptr->HasElement(WorldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity)
		{
			return ref _innerArchetype.ptr->GetElement<T>(WorldState, entity);
		}

		/// <summary>
		/// Возвращает элемент если он существует, если нет - создаёт новый
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity, out bool isExist)
		{
			return ref _innerArchetype.ptr->GetElement<T>(WorldState, entity, out isExist);
		}

		/// <summary>
		/// Возвращает элемент только если он существует
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetElement(Entity entity, out bool isExist)
		{
			return ref _innerArchetype.ptr->TryGetElement<T>(WorldState, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetElementValue(Entity entity, out T value)
		{
			value = _innerArchetype.ptr->TryGetElement<T>(WorldState, entity, out var isExist);
			return isExist;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_innerArchetype.ptr->Clear<T>(WorldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBackElement(Entity entity)
		{
			return _innerArchetype.ptr->RemoveSwapBackElement(WorldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_innerArchetype.ptr->Dispose(WorldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerator<ComponentSetElement<T>> GetEnumerator()
		{
			ref var elements = ref _innerArchetype.Value()._elements;
			var ptr = elements.GetValuePtr<ComponentSetElement<T>>(WorldState);

			return new MemListEnumerator<ComponentSetElement<T>>(ptr, _innerArchetype.ptr->Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerable<ComponentSetElement<T>> GetEnumerable()
		{
			return new MemListEnumerable<ComponentSetElement<T>>(GetEnumerator());
		}
	}
}
