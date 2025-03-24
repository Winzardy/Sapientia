using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator.My
{
	public unsafe partial struct Allocator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Align(int size) => ((size + 3) & ~3);

		public MemPtr MemAlloc(int size)
		{
			if (size == 0)
				return MemPtr.CreateZeroSized(allocatorId);

			var blockSize = Align(size + TSize<MemBlock>.size);
			var sizeId = (blockSize / MIN_BLOCK_SIZE).Log2();

			var blockRef = AllocateBlock(blockSize, sizeId);

			var result = new MemPtr(blockRef.memoryZoneId, blockRef.memoryZoneOffset + TSize<MemBlock>.size, allocatorId);

			return result;
		}

		public bool MemFree(MemPtr memPtr)
		{
			if (!memPtr.IsValid())
				return false;
			if (memPtr.IsZeroSized())
				return true;

			var blockRef = new MemoryBlockRef(memPtr.zoneId, memPtr.zoneOffset - TSize<MemBlock>.size);
			FreeBlock(blockRef);

			return true;
		}
	}
}
