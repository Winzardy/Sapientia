using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator.State
{
	public static unsafe class ArchetypeExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetDestroyHandler<THandler>(this ref CachedPtr<Archetype> archetypePtr, WorldState worldState) where THandler : unmanaged, IElementDestroyHandler
		{
			archetypePtr.GetValue(worldState).SetDestroyHandler<THandler>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype GetArchetype<TComponent>(this WorldState worldState) where TComponent : unmanaged, IComponent
		{
			return ref ServiceRegistryContext.Create<TComponent, Archetype>().GetService<Archetype>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<Archetype> GetArchetypePtr<TComponent>(this WorldState worldState) where TComponent : unmanaged, IComponent
		{
			return ServiceRegistryContext.Create<TComponent, Archetype>().GetServicePtr<Archetype>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype GetArchetype<TComponent>(this ref WorldId worldId) where TComponent : unmanaged, IComponent
		{
			return ref GetArchetype<TComponent>(worldId.GetWorldState());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<Archetype> GetArchetypePtr<TComponent>(this ref WorldId worldId) where TComponent : unmanaged, IComponent
		{
			return GetArchetypePtr<TComponent>(worldId.GetWorldState());
		}
	}
}
