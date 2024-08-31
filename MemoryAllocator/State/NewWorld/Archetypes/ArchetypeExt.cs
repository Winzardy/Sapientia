using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public static class ArchetypeExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetDestroyHandler<THandler>(this ref Ptr<Archetype> archetypePtr) where THandler : unmanaged, IElementDestroyHandler
		{
			archetypePtr.GetValue().SetDestroyHandler<THandler>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype GetArchetype<TComponent>(this ref AllocatorId allocatorId) where TComponent : unmanaged, IComponent
		{
			return ref allocatorId.GetServiceAs<TComponent, Archetype>();
		}
	}
}
