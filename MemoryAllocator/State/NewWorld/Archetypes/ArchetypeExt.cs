using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public static unsafe class ArchetypeExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetDestroyHandler<THandler>(this ref Ptr<Archetype> archetypePtr) where THandler : unmanaged, IElementDestroyHandler
		{
			archetypePtr.GetValue().SetDestroyHandler<THandler>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype<TComponent> GetArchetype<TComponent>(this ref Allocator allocator) where TComponent : unmanaged, IComponent
		{
			return ref allocator.GetServiceAs<TComponent, Archetype<TComponent>>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Archetype<TComponent>* GetArchetypePtr<TComponent>(this ref Allocator allocator) where TComponent : unmanaged, IComponent
		{
			return allocator.GetServiceAsPtr<TComponent, Archetype<TComponent>>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype<TComponent> GetArchetype<TComponent>(this ref AllocatorId allocatorId) where TComponent : unmanaged, IComponent
		{
			return ref allocatorId.GetServiceAs<TComponent, Archetype<TComponent>>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Archetype<TComponent>* GetArchetypePtr<TComponent>(this ref AllocatorId allocatorId) where TComponent : unmanaged, IComponent
		{
			return allocatorId.GetServiceAsPtr<TComponent, Archetype<TComponent>>();
		}
	}
}
