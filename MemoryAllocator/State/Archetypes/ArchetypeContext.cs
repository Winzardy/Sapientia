using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator.State
{
	public readonly unsafe struct ArchetypeContext<T> where T: unmanaged, IComponent
	{
		public readonly WorldState worldState;
		public readonly SafePtr<Archetype> innerArchetype;

		public ArchetypeContext(WorldState worldState) : this(worldState, worldState.GetArchetypePtr<T>())
		{
		}

		public ArchetypeContext(WorldState worldState, SafePtr<Archetype> innerArchetype)
		{
			this.worldState = worldState;
			this.innerArchetype = innerArchetype;
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => innerArchetype.ptr->Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDestroyHandler<THandler>() where THandler : unmanaged, IElementDestroyHandler
		{
			innerArchetype.ptr->SetDestroyHandler<THandler>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<ArchetypeElement<T>> GetRawElements()
		{
			return innerArchetype.ptr->GetRawElements<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<ArchetypeElement<T>> GetSpan()
		{
			return innerArchetype.ptr->GetSpan<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity)
		{
			return ref innerArchetype.ptr->ReadElement<T>(worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity, out bool isExist)
		{
			return ref innerArchetype.ptr->ReadElement<T>(worldState, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElementNoCheck(Entity entity)
		{
			return ref innerArchetype.ptr->ReadElementNoCheck<T>(worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<TEnumerable>(in TEnumerable entities) where TEnumerable: IEnumerable<Entity>
		{
			return innerArchetype.ptr->ReadElements<T, TEnumerable>(worldState, entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return innerArchetype.ptr->HasElement(worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity)
		{
			return ref innerArchetype.ptr->GetElement<T>(worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity, out bool isCreated)
		{
			return ref innerArchetype.ptr->GetElement<T>(worldState, entity, out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetElement(Entity entity, out bool isExist)
		{
			return ref innerArchetype.ptr->TryGetElement<T>(worldState, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetElementValue(Entity entity, out T value)
		{
			value = innerArchetype.ptr->TryGetElement<T>(worldState, entity, out var isExist);
			return isExist;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			innerArchetype.ptr->Clear<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			innerArchetype.ptr->ClearFast<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBackElement(Entity entity)
		{
			innerArchetype.ptr->RemoveSwapBackElement(worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			innerArchetype.ptr->Dispose(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<ArchetypeElement<T>> GetEnumerator()
		{
			ref var elements = ref innerArchetype.Value()._elements;
			var ptr = elements.GetValuePtr<ArchetypeElement<T>>(worldState);

			return new ListEnumerator<ArchetypeElement<T>>(ptr, innerArchetype.ptr->Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerable<ArchetypeElement<T>> GetEnumerable()
		{
			return new ListEnumerable<ArchetypeElement<T>>(GetEnumerator());
		}
	}
}
