using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public partial struct WorldState
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr(in MemPtr memPtr)
		{
			return GetAllocator().GetSafePtr(memPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetSafePtr<T>(in MemPtr memPtr) where T: unmanaged
		{
			return GetAllocator().GetSafePtr<T>(memPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr CopyPtrTo(WorldState dstWorldState, in MemPtr srsPtr)
		{
			return GetAllocator().CopyPtrTo(ref dstWorldState.GetAllocator(), srsPtr);
		}
	}
}
