using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public static class MemPtrWorldExt
	{
		[MethodImpl(256)]
		public static SafePtr GetPtr(this ref MemPtr memPtr, World world)
		{
			return world.GetSafePtr(memPtr);
		}

		[MethodImpl(256)]
		public static void Dispose(this ref MemPtr memPtr, World world)
		{
			world.MemFree(memPtr);
			memPtr = MemPtr.Invalid;
		}

		[MethodImpl(256)]
		public static MemPtr CopyTo(this ref MemPtr memPtr, World srsWorld, World dstWorld)
		{
			return srsWorld.CopyPtrTo(dstWorld, memPtr);
		}
	}
}
