namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct ArchetypePtr<T> where T: unmanaged, IComponent
	{
		private CachedPtr<Archetype> _archetypePtr;

		public ref Archetype GetArchetype(WorldState worldState)
		{
			return ref _archetypePtr.GetValue(worldState);
		}

		public ArchetypeContext<T> GetArchetypeContext(WorldState worldState)
		{
			return new ArchetypeContext<T>(worldState, _archetypePtr.GetPtr(worldState));
		}

		public static implicit operator ArchetypePtr<T>(CachedPtr<Archetype> ptr)
		{
			return new ArchetypePtr<T>
			{
				_archetypePtr = ptr,
			};
		}

		public static implicit operator CachedPtr<Archetype>(ArchetypePtr<T> archetypePtr)
		{
			return archetypePtr._archetypePtr;
		}

		public static implicit operator MemPtr(ArchetypePtr<T> archetypePtr)
		{
			return archetypePtr._archetypePtr.memPtr;
		}
	}
}
