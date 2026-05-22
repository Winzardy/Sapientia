using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.State;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct WorldState
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly ref ComponentsManager GetComponentsManager()
		{
			return ref WorldStateData.componentsManager;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<ComponentSet> GetComponentSetPtr<T>() where T : unmanaged, IComponent
		{
			return GetComponentsManager().GetComponentSet(this, TypeIdOf<IComponent, T>.typeId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref ComponentSet GetComponentSet<T>() where T : unmanaged, IComponent
		{
			return ref GetComponentSetPtr<T>().Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterComponentSet<T>(CachedPtr<ComponentSet> componentSetPtr) where T : unmanaged, IComponent
		{
			GetComponentsManager().RegisterComponentSet(this, TypeIdOf<IComponent, T>.typeId, componentSetPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasComponentSet<T>() where T : unmanaged, IComponent
		{
			return GetComponentsManager().HasComponentSet(this, TypeIdOf<IComponent, T>.typeId);
		}
	}
}
