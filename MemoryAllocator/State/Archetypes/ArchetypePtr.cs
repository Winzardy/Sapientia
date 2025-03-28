using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct ArchetypePtr<T> where T: unmanaged, IComponent
	{
		private Ptr<Archetype> _archetypePtr;

		public ref Archetype GetArchetype(SafePtr<Allocator> allocator)
		{
			return ref _archetypePtr.GetValue(allocator);
		}

		public ref Archetype GetArchetype()
		{
			return ref _archetypePtr.GetValue();
		}

		public ArchetypeContext<T> GetArchetypeContext(SafePtr<Allocator> allocator)
		{
			return new ArchetypeContext<T>(allocator, _archetypePtr.GetPtr(allocator));
		}

		public static implicit operator ArchetypePtr<T>(Ptr<Archetype> ptr)
		{
			return new ArchetypePtr<T>
			{
				_archetypePtr = ptr,
			};
		}

		public static implicit operator Ptr<Archetype>(ArchetypePtr<T> archetypePtr)
		{
			return archetypePtr._archetypePtr;
		}

		public static implicit operator MemPtr(ArchetypePtr<T> archetypePtr)
		{
			return archetypePtr._archetypePtr.memPtr;
		}
	}
}
