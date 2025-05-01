using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public partial class World
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr(in WPtr wPtr)
		{
			return _allocator.GetSafePtr(wPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetSafePtr<T>(in WPtr wPtr) where T: unmanaged
		{
			return _allocator.GetSafePtr<T>(wPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WPtr CopyPtrTo(World dstWorld, in WPtr srsPtr)
		{
			var dstMemPtr = _allocator.CopyPtrTo(ref dstWorld._allocator, srsPtr.allocatorPtr);
			return new WPtr(dstMemPtr, worldId);
		}
	}
}
