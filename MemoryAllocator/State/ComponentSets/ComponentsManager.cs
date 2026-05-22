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
	/// Пустой слот = <c>default(CachedPtr&lt;ComponentSet&gt;)</c>, проверка через <see cref="HasComponentSet"/>.
	/// </summary>
	public struct ComponentsManager
	{
		/// <summary>
		/// Слот <c>i</c> = <see cref="CachedPtr{T}"/> на <see cref="ComponentSet"/> для типа компонента
		/// с индексом <c>i</c> в контексте <see cref="IComponent"/>. Default-слот = не зарегистрирован.
		/// </summary>
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
			E.ASSERT(componentSets.IsCreated, "ComponentsManager не инициализирован: GetComponentSet до RegisterComponentSet");
			return componentSets[worldState, componentType].GetPtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref CachedPtr<ComponentSet> GetComponentSetRef(WorldState worldState, TypeId<IComponent> componentType)
		{
			E.ASSERT(componentSets.IsCreated, "ComponentsManager не инициализирован: GetComponentSetRef до RegisterComponentSet");
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
