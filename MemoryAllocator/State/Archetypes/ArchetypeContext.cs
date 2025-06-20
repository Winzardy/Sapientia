using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator.State
{
	public readonly unsafe struct ArchetypeContext<T> : IEnumerable<ArchetypeElement<T>> where T: unmanaged, IComponent
	{
		public readonly World world;
		public readonly SafePtr<Archetype> innerArchetype;

		public ArchetypeContext(World world) : this(world, world.GetArchetypePtr<T>())
		{
		}

		public ArchetypeContext(World world, SafePtr<Archetype> innerArchetype)
		{
			this.world = world;
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
			innerArchetype.ptr->SetDestroyHandler<THandler>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<ArchetypeElement<T>> GetRawElements()
		{
			return innerArchetype.ptr->GetRawElements<T>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<ArchetypeElement<T>> GetSpan()
		{
			return innerArchetype.ptr->GetSpan<T>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity)
		{
			return ref innerArchetype.ptr->ReadElement<T>(world, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity, out bool isExist)
		{
			return ref innerArchetype.ptr->ReadElement<T>(world, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElementNoCheck(Entity entity)
		{
			return ref innerArchetype.ptr->ReadElementNoCheck<T>(world, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<TEnumerable>(in TEnumerable entities) where TEnumerable: IEnumerable<Entity>
		{
			return innerArchetype.ptr->ReadElements<T, TEnumerable>(world, entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return innerArchetype.ptr->HasElement(world, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity)
		{
			return ref innerArchetype.ptr->GetElement<T>(world, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity, out bool isCreated)
		{
			return ref innerArchetype.ptr->GetElement<T>(world, entity, out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetElement(Entity entity, out bool isExist)
		{
			return ref innerArchetype.ptr->TryGetElement<T>(world, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			innerArchetype.ptr->Clear<T>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			innerArchetype.ptr->ClearFast<T>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBackElement(Entity entity)
		{
			innerArchetype.ptr->RemoveSwapBackElement(world, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			innerArchetype.ptr->Dispose(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<ArchetypeElement<T>> GetEnumerator()
		{
			ref var elements = ref innerArchetype.Value()._elements;
			var ptr = elements.GetValuePtr<ArchetypeElement<T>>(world);

			return new ListEnumerator<ArchetypeElement<T>>(ptr, innerArchetype.ptr->Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<ArchetypeElement<T>> IEnumerable<ArchetypeElement<T>>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
