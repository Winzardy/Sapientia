using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public partial class World
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr(in MemPtr memPtr)
		{
			return _allocator.GetSafePtr(memPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetSafePtr<T>(in MemPtr memPtr) where T: unmanaged
		{
			return _allocator.GetSafePtr<T>(memPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr CopyPtrTo(World dstWorld, in MemPtr srsPtr)
		{
			return _allocator.CopyPtrTo(ref dstWorld._allocator, srsPtr);
		}
	}
}
