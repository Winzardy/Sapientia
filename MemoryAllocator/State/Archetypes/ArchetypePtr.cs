using Sapientia.Data;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct ArchetypePtr<T> where T: unmanaged, IComponent
	{
		private CWPtr<Archetype> _archetypePtr;

		public ref Archetype GetArchetype(World world)
		{
			return ref _archetypePtr.GetValue(world);
		}

		public ref Archetype GetArchetype()
		{
			return ref _archetypePtr.GetValue();
		}

		public ArchetypeContext<T> GetArchetypeContext(World world)
		{
			return new ArchetypeContext<T>(world, _archetypePtr.GetPtr(world));
		}

		public static implicit operator ArchetypePtr<T>(CWPtr<Archetype> ptr)
		{
			return new ArchetypePtr<T>
			{
				_archetypePtr = ptr,
			};
		}

		public static implicit operator CWPtr<Archetype>(ArchetypePtr<T> archetypePtr)
		{
			return archetypePtr._archetypePtr;
		}

		public static implicit operator WPtr(ArchetypePtr<T> archetypePtr)
		{
			return archetypePtr._archetypePtr.wPtr;
		}
	}
}
