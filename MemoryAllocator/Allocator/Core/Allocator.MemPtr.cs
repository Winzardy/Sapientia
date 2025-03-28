using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetRef<T>(in MemPtr ptr) where T : unmanaged
		{
			return ref *(T*)GetSafePtr(ptr).ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetSafePtr<T>(in MemPtr memPtr) where T : unmanaged
		{
			return GetSafePtr(memPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr GetSafePtr(in MemPtr memPtr)
		{
			var memory = zonesList[memPtr.zoneId]->memory;
			var ptr = memory + memPtr.zoneOffset;
#if DEBUG
			var hiPtr = memory + ((MemoryBlock*)ptr - 1)->blockSize;
			return new SafePtr(ptr, hiPtr);
#else
			return new SafePtr(ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly byte* GetUnsafePtr(in MemPtr memPtr)
		{
			return zonesList[memPtr.zoneId]->memory + memPtr.zoneOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetPtrSize(in MemPtr memPtr)
		{
			return GetBlockSize(memPtr) - TSize<MemoryBlock>.size;
		}

		public MemPtr CopyPtrTo(SafePtr<Allocator> dstAllocator, MemPtr memPtr)
		{
			if (!memPtr.IsValid())
				return MemPtr.Invalid;
			if (memPtr.IsZeroSized())
			{
				memPtr.allocatorId = dstAllocator.Value().allocatorId;
				return memPtr;
			}

			var size = GetPtrSize(memPtr);
			var dstMemPtr = dstAllocator.Value().MemAlloc(size);

			var srcData = GetUnsafePtr(memPtr);
			var dstData = dstAllocator.Value().GetUnsafePtr(dstMemPtr);

			MemoryExt.MemCopy(srcData, dstData, size);

			return dstMemPtr;
		}
	}
}
