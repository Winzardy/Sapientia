using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public static class MemPtrWorldExt
	{
		[MethodImpl(256)]
		public static SafePtr GetPtr(this ref MemPtr memPtr, WorldState worldState)
		{
			return worldState.GetSafePtr(memPtr);
		}

		[MethodImpl(256)]
		public static void Dispose(this ref MemPtr memPtr, WorldState worldState)
		{
			worldState.MemFree(memPtr);
			memPtr = MemPtr.Invalid;
		}
	}
}
