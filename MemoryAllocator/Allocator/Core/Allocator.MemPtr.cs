using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetRef<T>(in MemPtr memPtr) where T : unmanaged
		{
			return ref *(T*)GetSafePtr(memPtr).ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetSafePtr<T>(in MemPtr memPtr) where T : unmanaged
		{
			return GetSafePtr(memPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr(in MemPtr memPtr)
		{
			var memory = _zonesList[memPtr.zoneId].memory;
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

		public MemPtr CopyPtrTo(ref Allocator dstAllocator, MemPtr srsMemPtr)
		{
			if (!srsMemPtr.IsValid())
				return MemPtr.Invalid;
			if (srsMemPtr.IsZeroSized())
			{
				return srsMemPtr;
			}

			var size = GetPtrSize(srsMemPtr);
			var dstMemPtr = dstAllocator.MemAlloc(size);

			var srcData = GetSafePtr(srsMemPtr);
			var dstData = dstAllocator.GetSafePtr(dstMemPtr);

			MemoryExt.MemCopy(srcData, dstData, size);

			return dstMemPtr;
		}
	}
}
