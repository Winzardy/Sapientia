using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial class Allocator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetRef<T>(in MemPtr ptr) where T : unmanaged
		{
			return ref *(T*)GetSafePtr(ptr).ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetSafePtr<T>(in MemPtr memPtr) where T : unmanaged
		{
			return GetSafePtr(memPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr(in MemPtr memPtr)
		{
			var memory = zonesList[memPtr.zoneId].ptr->memory;
			var safePtr = memory + memPtr.zoneOffset;
#if DEBUG
			var size = (safePtr.Cast<MemoryBlock>() - 1).ptr->blockSize - TSize<MemoryBlock>.size;
			return new SafePtr(safePtr.ptr, size);
#else
			return new SafePtr(safePtr.ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetPtrSize(in MemPtr memPtr)
		{
			return GetBlockSize(memPtr) - TSize<MemoryBlock>.size;
		}

		public MemPtr CopyPtrTo(Allocator dstAllocator, MemPtr memPtr)
		{
			if (!memPtr.IsCreated())
				return MemPtr.Invalid;
			if (memPtr.IsZeroSized())
			{
				memPtr.allocatorId = dstAllocator.allocatorId;
				return memPtr;
			}

			var size = GetPtrSize(memPtr);
			var dstMemPtr = dstAllocator.MemAlloc(size);

			var srcData = GetSafePtr(memPtr);
			var dstData = dstAllocator.GetSafePtr(dstMemPtr);

			MemoryExt.MemCopy(srcData, dstData, size);

			return dstMemPtr;
		}
	}
}
