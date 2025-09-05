namespace Sapientia.MemoryAllocator.State
{
	public struct ComponentSetPtr<T> where T: unmanaged, IComponent
	{
		private CachedPtr<ComponentSet> _archetypePtr;

		public ref ComponentSet GetArchetype(WorldState worldState)
		{
			return ref _archetypePtr.GetValue(worldState);
		}

		public ComponentSetContext<T> GetArchetypeContext(WorldState worldState)
		{
			return new ComponentSetContext<T>(worldState, _archetypePtr.GetPtr(worldState));
		}

		public static implicit operator ComponentSetPtr<T>(CachedPtr<ComponentSet> ptr)
		{
			return new ComponentSetPtr<T>
			{
				_archetypePtr = ptr,
			};
		}

		public static implicit operator CachedPtr<ComponentSet>(ComponentSetPtr<T> archetypePtr)
		{
			return archetypePtr._archetypePtr;
		}

		public static implicit operator MemPtr(ComponentSetPtr<T> archetypePtr)
		{
			return archetypePtr._archetypePtr.memPtr;
		}
	}
}
