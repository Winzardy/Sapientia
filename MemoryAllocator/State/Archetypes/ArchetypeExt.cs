using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator.State
{
	public static unsafe class ArchetypeExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetDestroyHandler<THandler>(this ref CWPtr<Archetype> archetypePtr, World world) where THandler : unmanaged, IElementDestroyHandler
		{
			archetypePtr.GetValue(world).SetDestroyHandler<THandler>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype GetArchetype<TComponent>(this World world) where TComponent : unmanaged, IComponent
		{
			return ref ServiceRegistryContext.Create<TComponent, Archetype>().GetService<Archetype>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<Archetype> GetArchetypePtr<TComponent>(this World world) where TComponent : unmanaged, IComponent
		{
			return ServiceRegistryContext.Create<TComponent, Archetype>().GetServicePtr<Archetype>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Archetype GetArchetype<TComponent>(this ref WorldId worldId) where TComponent : unmanaged, IComponent
		{
			return ref GetArchetype<TComponent>(worldId.GetWorld());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<Archetype> GetArchetypePtr<TComponent>(this ref WorldId worldId) where TComponent : unmanaged, IComponent
		{
			return GetArchetypePtr<TComponent>(worldId.GetWorld());
		}
	}
}
