using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetRef<T>(in AllocatorPtr allocatorPtr) where T : unmanaged
		{
			return ref *(T*)GetSafePtr(allocatorPtr).ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetSafePtr<T>(in AllocatorPtr allocatorPtr) where T : unmanaged
		{
			return GetSafePtr(allocatorPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr(in AllocatorPtr allocatorPtr)
		{
			var memory = _zonesList[allocatorPtr.zoneId].ptr->memory;
			var safePtr = memory + allocatorPtr.zoneOffset;
#if DEBUG
			var size = (safePtr.Cast<MemoryBlock>() - 1).ptr->blockSize - TSize<MemoryBlock>.size;
			return new SafePtr(safePtr.ptr, size);
#else
			return new SafePtr(safePtr.ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetPtrSize(in AllocatorPtr allocatorPtr)
		{
			return GetBlockSize(allocatorPtr) - TSize<MemoryBlock>.size;
		}

		public AllocatorPtr CopyPtrTo(ref Allocator dstAllocator, AllocatorPtr srsAllocatorPtr)
		{
			if (!srsAllocatorPtr.IsValid())
				return AllocatorPtr.Invalid;
			if (srsAllocatorPtr.IsZeroSized())
			{
				return srsAllocatorPtr;
			}

			var size = GetPtrSize(srsAllocatorPtr);
			var dstMemPtr = dstAllocator.MemAlloc(size);

			var srcData = GetSafePtr(srsAllocatorPtr);
			var dstData = dstAllocator.GetSafePtr(dstMemPtr);

			MemoryExt.MemCopy(srcData, dstData, size);

			return dstMemPtr;
		}
	}
}
