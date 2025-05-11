namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct ArchetypePtr<T> where T: unmanaged, IComponent
	{
		private CachedPtr<Archetype> _archetypePtr;

		public ref Archetype GetArchetype(World world)
		{
			return ref _archetypePtr.GetValue(world);
		}

		public ArchetypeContext<T> GetArchetypeContext(World world)
		{
			return new ArchetypeContext<T>(world, _archetypePtr.GetPtr(world));
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
