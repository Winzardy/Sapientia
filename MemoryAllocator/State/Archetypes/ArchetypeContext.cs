using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;

namespace Sapientia.MemoryAllocator.State
{
	public readonly unsafe struct ArchetypeContext<T> : IEnumerable<T> where T: unmanaged, IComponent
	{
		public readonly Allocator* allocator;
		public readonly Archetype* innerArchetype;

		public ArchetypeContext(Allocator* allocator) : this(allocator, ArchetypeExt.GetArchetypePtr<T>(allocator))
		{
		}

		public ArchetypeContext(Allocator* allocator, Archetype* innerArchetype)
		{
			this.allocator = allocator;
			this.innerArchetype = innerArchetype;
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => innerArchetype->Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDestroyHandler<THandler>() where THandler : unmanaged, IElementDestroyHandler
		{
			innerArchetype->SetDestroyHandler<THandler>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ArchetypeElement<T>* GetRawElements()
		{
			return innerArchetype->GetRawElements<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity)
		{
			return ref innerArchetype->ReadElement<T>(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity, out bool isExist)
		{
			return ref innerArchetype->ReadElement<T>(allocator, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElementNoCheck(Entity entity)
		{
			return ref innerArchetype->ReadElementNoCheck<T>(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<TEnumerable>(in TEnumerable entities) where TEnumerable: IEnumerable<Entity>
		{
			return innerArchetype->ReadElements<T, TEnumerable>(allocator, entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return innerArchetype->HasElement(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity)
		{
			return ref innerArchetype->GetElement<T>(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity, out bool isCreated)
		{
			return ref innerArchetype->GetElement<T>(allocator, entity, out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			innerArchetype->Clear<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			innerArchetype->ClearFast<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBackElement(Entity entity)
		{
			innerArchetype->RemoveSwapBackElement(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			innerArchetype->Dispose(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator()
		{
			return new ListEnumerator<T>(innerArchetype->_elements.GetValuePtr<T>(allocator), innerArchetype->Count);
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
