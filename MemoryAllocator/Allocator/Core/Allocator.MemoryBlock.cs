using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public partial struct Allocator
	{
		// Размер обязан быть кратен BLOCK_ALIGN: данные блока лежат сразу за этой структурой, поэтому
		// её размер и есть выравнивание выдаваемых указателей. Без Size DEBUG-сборка давала 20 байт
		// (8+4+4+4) — и любое 64-битное поле в выданной памяти садилось на офсет, кратный 4, что на
		// 32-битном ARM читается как SIGBUS.
		[StructLayout(LayoutKind.Sequential, Size = SIZE)]
		public struct MemoryBlock
		{
#if DEBUG
			private const int SIZE = 24;
#else
			private const int SIZE = 16;
#endif

			public BlockId id;

			public int prevBlockOffset; // Смещение к предыдущему блоку. Меньше нуля, иначе предыдущего блока нет.
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
