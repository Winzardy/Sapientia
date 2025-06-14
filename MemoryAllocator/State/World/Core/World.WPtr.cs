using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public partial struct World
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
		public MemPtr CopyPtrTo(World dstWorld, in MemPtr srsPtr)
		{
			return GetAllocator().CopyPtrTo(ref dstWorld.GetAllocator(), srsPtr);
		}
	}
}
