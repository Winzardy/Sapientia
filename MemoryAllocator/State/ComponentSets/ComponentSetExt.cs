using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator.State
{
	public static class ComponentSetExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetDestroyHandler<THandler>(this ref CachedPtr<ComponentSet> componentSetPtr, WorldState worldState) where THandler : unmanaged, IElementDestroyHandler
		{
			componentSetPtr.GetValue(worldState).SetDestroyHandler<THandler>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref ComponentSet GetComponentSet<TComponent>(this WorldState worldState) where TComponent : unmanaged, IComponent
		{
			return ref ServiceRegistryContext.Create<TComponent, ComponentSet>().GetService<ComponentSet>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<ComponentSet> GetComponentSetPtr<TComponent>(this WorldState worldState) where TComponent : unmanaged, IComponent
		{
			return ServiceRegistryContext.Create<TComponent, ComponentSet>().GetServicePtr<ComponentSet>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref ComponentSet GetComponentSet<TComponent>(this ref WorldId worldId) where TComponent : unmanaged, IComponent
		{
			return ref GetComponentSet<TComponent>(worldId.GetWorldState());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<ComponentSet> GetComponentSetPtr<TComponent>(this ref WorldId worldId) where TComponent : unmanaged, IComponent
		{
			return GetComponentSetPtr<TComponent>(worldId.GetWorldState());
		}
	}
}
