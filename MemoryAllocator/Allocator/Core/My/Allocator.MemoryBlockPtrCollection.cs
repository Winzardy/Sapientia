using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator.My
{
	public unsafe partial struct Allocator
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct MemoryBlockPtrCollection : IDisposable
		{
			public readonly int blockSize;
			public UnsafeList<MemoryBlockRef> freeBlocks;

			public int Count
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => freeBlocks.count;
			}

			public MemoryBlockPtrCollection(int blockSize, int capacity = 8)
			{
				this.blockSize = blockSize;
				this.freeBlocks = new UnsafeList<MemoryBlockRef>(capacity);
			}

			public void Add(MemoryBlockRef memoryBlock)
			{
				freeBlocks.Add(memoryBlock);
			}

			public void Dispose()
			{
				freeBlocks.Dispose();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetBlockSize(in MemPtr memPtr)
		{
			var blockPtr = zonesList[memPtr.zoneId].memory + memPtr.zoneOffset - TSize<MemoryBlock>.size;
			return blockPtr->blockSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef RemoveFreeBlock(BlockId blockId)
		{
			ref var freeBlocks = ref freeBlocksCollections[blockId.sizeId].freeBlocks;
			var result = freeBlocks.RemoveAtSwapBack(blockId.freeId);

			// Если ссылка на блок была перемещена, то меняем freeId блока этой ссылки
			if (freeBlocksCollections.count > blockId.freeId)
			{
				var blockRef = freeBlocks[blockId.freeId];
				ref var zone = ref zonesList[blockRef.memoryZoneId];

				var blockPtr = zone.memory + blockRef.memoryZoneOffset;
				blockPtr->id.freeId = blockId.freeId;
			}

			return result;
		}

		private MemoryBlockRef AllocateBlock(int requiredBlockSize, int sizeId)
		{
			E.ASSERT(sizeId >= 0 && sizeId < freeBlocksCollections.count);

			ref var blocksCollection = ref freeBlocksCollections[sizeId];
			var blocksCount = blocksCollection.Count;

			// Ищем свободный блок памяти, подходящий под требуемый размер
			while (blocksCount < 0)
			{
				sizeId++;

				// Если такого блока нет, то аллоцируем ещё одну зону
				if (sizeId >= freeBlocksCollections.count)
				{
					AllocateMemoryZone();
					sizeId = freeBlocksCollections.count - 1;
				}

				blocksCollection = ref freeBlocksCollections[sizeId];
				blocksCount = blocksCollection.Count;
			}

			// Берём последний свободный блок
			var blockRef = blocksCollection.freeBlocks.RemoveLast();
			// Получаем память блока в зоне
			ref var zone = ref zonesList[blockRef.memoryZoneId];
			var blockPtr = zone.memory + blockRef.memoryZoneOffset;
			// Маркируем блок занятым
			blockPtr->id.freeId = -1;

			// Вычисляем, осталось ли ещё свободное место у блока
			var extraSize = blockPtr->blockSize - requiredBlockSize;
			if (extraSize >= MIN_BLOCK_SIZE)
			{
				var extraSizeId = extraSize.Log2();

				// Создаём свободный блок из остатка памяти
				var extraBlockPtr = blockPtr + requiredBlockSize;
				extraBlockPtr->id = new BlockId(extraSizeId, blocksCollection.freeBlocks.count);
				extraBlockPtr->prevBlockOffset = -requiredBlockSize;
				extraBlockPtr->blockSize = extraSize;

				// Добавляем ссылку на новый блок в список свободных блоков
				var extraBlockRef = new MemoryBlockRef(blockRef.memoryZoneId, blockRef.memoryZoneOffset + requiredBlockSize);
				blocksCollection.freeBlocks.Add(extraBlockRef);

				// Если существует следующий блок, то устанавливаем ему смещение но новый "предыдущий блок"
				var nextBlockOffset = blockRef.memoryZoneOffset + blockPtr->blockSize;
				if (nextBlockOffset < zone.size)
				{
					var nextBlock = zone.memory + nextBlockOffset;
					nextBlock->prevBlockOffset = -extraSize;
				}

				// Устанавливаем новый размер блока
				blockPtr->blockSize = requiredBlockSize;
			}

			return blockRef;
		}

		private void FreeBlock(MemoryBlockRef memoryBlockRef)
		{
			E.ASSERT(memoryBlockRef.memoryZoneId >= 0 && memoryBlockRef.memoryZoneId < zonesList.count);

			ref var zone = ref zonesList[memoryBlockRef.memoryZoneId];
			E.ASSERT(zone.size > memoryBlockRef.memoryZoneOffset);

			var blockPtr = zone.memory + memoryBlockRef.memoryZoneOffset;
			E.ASSERT(zone.size >= memoryBlockRef.memoryZoneOffset + blockPtr->blockSize);

			// Если предыдущий блок пустой, то мерджим предыдущий блок
			var prevBlockPtr = blockPtr - blockPtr->prevBlockOffset;
			if (prevBlockPtr->id.freeId >= 0)
			{
				prevBlockPtr->blockSize += blockPtr->blockSize;
				RemoveFreeBlock(blockPtr->id);

				// Если следующий блок существует и он пустой, то мерджим следующий блок
				if (zone.size > memoryBlockRef.memoryZoneOffset + blockPtr->blockSize)
				{
					var nextBlockPtr = blockPtr + blockPtr->blockSize;
					if (nextBlockPtr->id.freeId >= 0)
					{
						prevBlockPtr->blockSize += nextBlockPtr->blockSize;
						RemoveFreeBlock(nextBlockPtr->id);
					}
				}

				// Если размер изменился, то перемещаем ссылку на блок в другую коллекцию
				var newBlockSizeId = prevBlockPtr->blockSize.Log2();
				if (prevBlockPtr->id.sizeId != newBlockSizeId)
				{
					var prevBlockRef = RemoveFreeBlock(prevBlockPtr->id);
					prevBlockPtr->id.sizeId = newBlockSizeId;

					freeBlocksCollections[prevBlockPtr->id.sizeId].Add(prevBlockRef);
				}
			}
			else
			{
				// Если следующий блок существует и он пустой, то мерджим следующий блок
				if (zone.size > memoryBlockRef.memoryZoneOffset + blockPtr->blockSize)
				{
					var nextBlockPtr = blockPtr + blockPtr->blockSize;
					if (nextBlockPtr->id.freeId >= 0)
					{
						blockPtr->blockSize += nextBlockPtr->blockSize;
						RemoveFreeBlock(nextBlockPtr->id);

						// Если размер изменился, то меняем sizeId
						blockPtr->id.sizeId = blockPtr->blockSize.Log2();
					}
				}

				// Восстанавливаем блок как свободный
				freeBlocksCollections[blockPtr->id.sizeId].Add(memoryBlockRef);
			}
		}
	}
}
