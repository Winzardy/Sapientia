using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sapientia.MemoryAllocator.My
{
	public unsafe partial struct Allocator
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct MemoryBlock
		{
			public BlockId id;

			public int prevBlockOffset;
			public int blockSize; // Равен размеру структуры MemoryBlock + размер свободной памяти блока

			public MemoryBlock(BlockId id, int prevBlockOffset, int blockSize)
			{
				this.id = id;
				this.prevBlockOffset = prevBlockOffset;
				this.blockSize = blockSize;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static MemoryBlock CreateFirstBlock(BlockId id, int zoneSize)
			{
				return new MemoryBlock(id, 0, zoneSize);
			}
		}
	}
}
