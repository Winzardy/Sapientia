using System.Runtime.CompilerServices;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator.State
{
	public static unsafe class ArchetypeExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetDestroyHandler<THandler>(this ref Ptr<Archetype> archetypePtr) where THandler : unmanaged, IElementDestroyHandler
		{
			archetypePtr.GetValue().SetDestroyHandler<THandler>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetDestroyHandler<THandler>(this ref Ptr<Archetype> archetypePtr, Allocator* allocator) where THandler : unmanaged, IElementDestroyHandler
		{
			archetypePtr.GetValue().SetDestroyHandler<THandler>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype GetArchetype<TComponent>(Allocator* allocator) where TComponent : unmanaged, IComponent
		{
			return ref ServiceRegistryContext.Create<TComponent, Archetype>().GetService<Archetype>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Archetype* GetArchetypePtr<TComponent>(Allocator* allocator) where TComponent : unmanaged, IComponent
		{
			return ServiceRegistryContext.Create<TComponent, Archetype>().GetServicePtr<Archetype>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype GetArchetype<TComponent>(this ref Allocator allocator) where TComponent : unmanaged, IComponent
		{
			return ref GetArchetype<TComponent>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Archetype* GetArchetypePtr<TComponent>(this ref Allocator allocator) where TComponent : unmanaged, IComponent
		{
			return GetArchetypePtr<TComponent>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype GetArchetype<TComponent>(this ref AllocatorId allocatorId) where TComponent : unmanaged, IComponent
		{
			return ref GetArchetype<TComponent>(allocatorId.GetAllocatorPtr());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Archetype* GetArchetypePtr<TComponent>(this ref AllocatorId allocatorId) where TComponent : unmanaged, IComponent
		{
			return GetArchetypePtr<TComponent>(allocatorId.GetAllocatorPtr());
		}
	}
}
