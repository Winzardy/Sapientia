using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetRef<T>(in MemPtr ptr) where T : unmanaged
		{
			return ref *(T*)GetUnsafePtr(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly byte* GetUnsafePtr(in MemPtr memPtr)
		{
			return ((byte*)zonesList[memPtr.zoneId].memory) + memPtr.zoneOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetPtrSize(in MemPtr memPtr)
		{
			return GetBlockSize(memPtr) - TSize<MemoryBlock>.size;
		}

		public MemPtr CopyPtrTo(Allocator* dstAllocator, MemPtr memPtr)
		{
			if (!memPtr.IsValid())
				return MemPtr.Invalid;
			if (memPtr.IsZeroSized())
			{
				memPtr.allocatorId = dstAllocator->allocatorId;
				return memPtr;
			}

			var size = GetPtrSize(memPtr);
			var dstMemPtr = dstAllocator->MemAlloc(size);

			var srcData = GetUnsafePtr(memPtr);
			var dstData = dstAllocator->GetUnsafePtr(dstMemPtr);

			MemoryExt.MemCopy(srcData, dstData, size);

			return dstMemPtr;
		}
	}
}
