using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.MemoryAllocator.Collections;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public unsafe struct Archetype<T> : IEnumerable<T> where T: unmanaged, IComponent
	{
		public Archetype innerArchetype;

		public uint Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => innerArchetype.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype<T> RegisterArchetype(AllocatorId allocatorId, uint elementsCount)
		{
			return RegisterArchetype(ref allocatorId.GetAllocator(), elementsCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype<T> RegisterArchetype(ref Allocator allocator, uint elementsCount)
		{
			return RegisterArchetype(ref allocator, elementsCount, allocator.serviceLocator.GetService<EntitiesStatePart>().EntitiesCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype<T> RegisterArchetype(ref Allocator allocator, uint elementsCount, uint entitiesCapacity)
		{
			return Archetype.RegisterArchetype<T>(ref allocator, elementsCount, entitiesCapacity).ToCachedPtr<Archetype<T>>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDestroyHandler<THandler>() where THandler : unmanaged, IElementDestroyHandler
		{
			innerArchetype.SetDestroyHandler<THandler>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ArchetypeElement<T>* GetRawElements()
		{
			return innerArchetype.GetRawElements<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadElement(Entity entity)
		{
			return ref innerArchetype.ReadElement<T>(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<T> ReadElements<TEnumerable>(in TEnumerable entities) where TEnumerable: IEnumerable<Entity>
		{
			return innerArchetype.ReadElements<T, TEnumerable>(entities);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return innerArchetype.HasElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetElement(Entity entity)
		{
			return ref innerArchetype.GetElement<T>(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			innerArchetype.Clear<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			innerArchetype.ClearFast<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBackElement(Entity entity)
		{
			innerArchetype.RemoveSwapBackElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			innerArchetype.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SparseSet.Enumerator<T> GetEnumerator()
		{
			return new SparseSet.Enumerator<T>(innerArchetype._elements.GetAllocator(), ref innerArchetype._elements);
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
