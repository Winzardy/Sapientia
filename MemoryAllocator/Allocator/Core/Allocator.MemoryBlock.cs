using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public partial struct Allocator
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct MemoryBlock
		{
			public BlockId id;

			public int prevBlockOffset;
			public int blockSize; // Равен размеру структуры MemoryBlock + размер свободной памяти блока
#if DEBUG
			public int dataSize; // Размер, который был запрошен при аллокации
#endif

			public bool IsStartBlock
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => prevBlockOffset >= 0;
			}

			public MemoryBlock(BlockId id, int prevBlockOffset, int blockSize)
			{
				this.id = id;
				this.prevBlockOffset = prevBlockOffset;
				this.blockSize = blockSize;
#if DEBUG
				this.dataSize = 0;
#endif
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static MemoryBlock CreateFirstBlock(BlockId id, int zoneSize)
			{
				return new MemoryBlock(id, 0, zoneSize);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetBlockSizeId(int blockSize)
		{
			return blockSize.Log2() - MIN_BLOCK_SIZE_LOG2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetBlockSizeId(int blockSize, out int roundedUpSizeId)
		{
			var log2= blockSize.Log2();
			var sizeId = log2 - MIN_BLOCK_SIZE_LOG2;
			roundedUpSizeId = sizeId;

			if ((1 << log2) != blockSize)
				roundedUpSizeId++;

			return sizeId;
		}
	}
}
