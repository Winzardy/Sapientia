using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator.State
{
	public readonly unsafe struct ArchetypeContext<T> : IEnumerable<T> where T: unmanaged, IComponent
	{
		public readonly Allocator allocator;
		public readonly SafePtr<Archetype> innerArchetype;

		public ArchetypeContext(Allocator allocator) : this(allocator, allocator.GetArchetypePtr<T>())
		{
		}

		public ArchetypeContext(Allocator allocator, SafePtr<Archetype> innerArchetype)
		{
			this.allocator = allocator;
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
			innerArchetype.ptr->SetDestroyHandler<THandler>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<ArchetypeElement<T>> GetRawElements()
		{
			return innerArchetype.ptr->GetRawElements<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity)
		{
			return ref innerArchetype.ptr->ReadElement<T>(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity, out bool isExist)
		{
			return ref innerArchetype.ptr->ReadElement<T>(allocator, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElementNoCheck(Entity entity)
		{
			return ref innerArchetype.ptr->ReadElementNoCheck<T>(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<TEnumerable>(in TEnumerable entities) where TEnumerable: IEnumerable<Entity>
		{
			return innerArchetype.ptr->ReadElements<T, TEnumerable>(allocator, entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return innerArchetype.ptr->HasElement(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity)
		{
			return ref innerArchetype.ptr->GetElement<T>(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity, out bool isCreated)
		{
			return ref innerArchetype.ptr->GetElement<T>(allocator, entity, out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetElement(Entity entity, out bool isExist)
		{
			return ref innerArchetype.ptr->TryGetElement<T>(allocator, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			innerArchetype.ptr->Clear<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			innerArchetype.ptr->ClearFast<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBackElement(Entity entity)
		{
			innerArchetype.ptr->RemoveSwapBackElement(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			innerArchetype.ptr->Dispose(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator()
		{
			return new ListEnumerator<T>(innerArchetype.ptr->_elements.GetValuePtr<T>(allocator), innerArchetype.ptr->Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
