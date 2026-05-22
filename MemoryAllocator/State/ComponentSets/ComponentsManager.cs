using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Менеджер всех <see cref="ComponentSet"/> мира. Хранит компоненты в плотном массиве
	/// проиндексированном по <see cref="TypeId{IComponent}"/>.
	/// Размер массива равен <see cref="TypeId{IComponent}.Count"/> — количество всех типов
	/// реализующих <see cref="IComponent"/> в проекте, посчитанное генератором TypeIndexer'а.
	/// </summary>
	public struct ComponentsManager
	{
		public MemArray<CachedPtr<ComponentSet>> componentSets;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureInitialized(WorldState worldState)
		{
			if (!componentSets.IsCreated)
				componentSets = new MemArray<CachedPtr<ComponentSet>>(worldState, TypeId<IComponent>.Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<ComponentSet> GetComponentSet(WorldState worldState, TypeId<IComponent> componentType)
		{
			EnsureInitialized(worldState);
			return componentSets[worldState, componentType].GetPtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref CachedPtr<ComponentSet> GetComponentSetRef(WorldState worldState, TypeId<IComponent> componentType)
		{
			EnsureInitialized(worldState);
			return ref componentSets[worldState, componentType];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterComponentSet(WorldState worldState, TypeId<IComponent> componentType, CachedPtr<ComponentSet> componentSetPtr)
		{
			EnsureInitialized(worldState);
			componentSets[worldState, componentType] = componentSetPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasComponentSet(WorldState worldState, TypeId<IComponent> componentType)
		{
			if (!componentSets.IsCreated)
				return false;
			return componentSets[worldState, componentType].IsValid();
		}
	}
}
