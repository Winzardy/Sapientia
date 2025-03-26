using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
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

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public MemoryBlockPtrCollection(int blockSize, int capacity = 8)
			{
				this.blockSize = blockSize;
				this.freeBlocks = new UnsafeList<MemoryBlockRef>(capacity);
			}


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				freeBlocks.Dispose();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetBlockSize(in MemPtr memPtr)
		{
			var blockPtr = (MemoryBlock*)(zonesList[memPtr.zoneId].memory + memPtr.zoneOffset - TSize<MemoryBlock>.size);
			return blockPtr->blockSize;
		}

		private void CreateFreeBlock(ref MemoryZone memoryZone, MemoryBlock* blockPtr, MemoryBlockRef blockRef, int blockSize, int prevBlockOffset)
		{
			var sizeId = blockSize.Log2() - MIN_BLOCK_SIZE_LOG2;

			// Иногда возвращается блок больше 2^(sizeId + MIN_BLOCK_SIZE_LOG2)
			// Это возникает если размер зоны не степень двойки
			if (sizeId >= freeBlocksCollections.count)
				sizeId = freeBlocksCollections.count - 1;

			ref var sizeBlocksCollection = ref freeBlocksCollections[sizeId];

			E.ASSERT(sizeBlocksCollection.blockSize <= blockSize);

			// Создаём свободный блок из остатка памяти
			blockPtr->blockSize = blockSize;
			blockPtr->prevBlockOffset = prevBlockOffset;
			blockPtr->id = new BlockId(sizeId, sizeBlocksCollection.freeBlocks.count);

			// Добавляем ссылку на новый блок в список свободных блоков
			sizeBlocksCollection.freeBlocks.Add(blockRef);

			var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->blockSize);
			if (memoryZone.memory + memoryZone.size > nextBlockPtr)
				nextBlockPtr->prevBlockOffset = -blockPtr->blockSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AddFreeBlock(MemoryBlock* blockPtr, MemoryBlockRef blockRef)
		{
			ref var freeBlocksCollection = ref freeBlocksCollections[blockPtr->id.sizeId];

			E.ASSERT(blockPtr->blockSize >= freeBlocksCollection.blockSize);

			ref var freeBlocks = ref freeBlocksCollection.freeBlocks;
			blockPtr->id.freeId = freeBlocks.count;

			freeBlocks.Add(blockRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef RemoveFreeBlock(BlockId blockId)
		{
			if (blockId.sizeId >= freeBlocksCollections.count)
				;
			E.ASSERT(blockId.sizeId < freeBlocksCollections.count);

			ref var freeBlocks = ref freeBlocksCollections[blockId.sizeId].freeBlocks;
			var result = freeBlocks.RemoveAtSwapBack(blockId.freeId);

			// Если ссылка на блок была перемещена, то меняем freeId блока этой ссылки
			if (freeBlocksCollections.count > blockId.freeId)
			{
				var blockRef = freeBlocks[blockId.freeId];
				ref var zone = ref zonesList[blockRef.memoryZoneId];

				var blockPtr = (MemoryBlock*)(zone.memory + blockRef.memoryZoneOffset);
				blockPtr->id.freeId = blockId.freeId;
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef AllocateBlock(int requiredBlockSize, out MemoryBlock* blockPtr)
		{
			// `sizeId` округлён вверх, т.к. хотим найти блок не меньше требуемого размера
			// `requiredSizeId` - это индекс для размера `requiredBlockSize`
			var requiredSizeId = requiredBlockSize.Log2(out var sizeId) - MIN_BLOCK_SIZE_LOG2;

			E.ASSERT(sizeId >= 0 && sizeId < freeBlocksCollections.count);

			ref var blocksCollection = ref freeBlocksCollections[sizeId];

			// Ищем свободный блок памяти, подходящий под требуемый размер
			while (blocksCollection.Count == 0)
			{
				sizeId++;

				// Если такого блока нет, то аллоцируем ещё одну зону
				if (sizeId >= freeBlocksCollections.count)
				{
					AllocateMemoryZone();
					// При аллокации зоны аллоцируется блок самого большого размера
					sizeId = freeBlocksCollections.count - 1;
				}

				blocksCollection = ref freeBlocksCollections[sizeId];
			}

			// Берём последний свободный блок
			var blockRef = blocksCollection.freeBlocks.RemoveLast();
			// Получаем память блока в зоне
			ref var zone = ref zonesList[blockRef.memoryZoneId];
			blockPtr = (MemoryBlock*)(zone.memory + blockRef.memoryZoneOffset);
			// Маркируем блок занятым
			blockPtr->id.freeId = -1;

			E.ASSERT(blockPtr->id.sizeId == sizeId);

			// Вычисляем, осталось ли ещё свободное место у блока
			var extraSize = blockPtr->blockSize - requiredBlockSize;
			E.ASSERT(extraSize >= 0);

			if (extraSize >= MIN_BLOCK_SIZE)
			{
				var extraBlockPtr = (MemoryBlock*)((byte*)blockPtr + requiredBlockSize);
				var extraBlockRef = new MemoryBlockRef(blockRef.memoryZoneId, blockRef.memoryZoneOffset + requiredBlockSize);

				CreateFreeBlock(ref zone, extraBlockPtr, extraBlockRef, extraSize, -requiredBlockSize);

				// Устанавливаем новый размер блока
				blockPtr->blockSize = requiredBlockSize;
				blockPtr->id.sizeId = requiredSizeId;
			}

			return blockRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef ReAllocateBlock(MemoryBlockRef blockRef, int requiredBlockSize, out MemoryBlock* blockPtr)
		{
			ref var zone = ref zonesList[blockRef.memoryZoneId];
			blockPtr = (MemoryBlock*)(zone.memory + blockRef.memoryZoneOffset);

			if (blockPtr->blockSize >= requiredBlockSize)
				return blockRef;

			E.ASSERT(blockPtr->blockSize > 0);

			if (blockRef.memoryZoneOffset + blockPtr->blockSize < zone.size)
			{
				var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->blockSize);
				if (nextBlockPtr->blockSize > 0 && nextBlockPtr->id.IsFree)
				{
					var requiredSize = requiredBlockSize - blockPtr->blockSize;
					var extraSize = nextBlockPtr->blockSize - requiredSize;
					if (extraSize >= 0)
					{
						RemoveFreeBlock(nextBlockPtr->id);
						blockPtr->blockSize += requiredSize;

						// Если остаток меньше минимального размера блока, то оставляем остаток памяти в текущем блоке
						if (extraSize < MIN_BLOCK_SIZE)
						{
							blockPtr->blockSize += extraSize;

							nextBlockPtr = (MemoryBlock*)((byte*)nextBlockPtr + nextBlockPtr->blockSize);
							if (zone.memory + zone.size > nextBlockPtr)
								nextBlockPtr->prevBlockOffset = -blockPtr->blockSize;
						}
						else
						{
							var extraBlockPtr = (MemoryBlock*)((byte*)blockPtr + requiredBlockSize);
							var extraBlockRef = new MemoryBlockRef(blockRef.memoryZoneId, blockRef.memoryZoneOffset + blockPtr->blockSize);
							CreateFreeBlock(ref zone, extraBlockPtr, extraBlockRef, extraSize, -blockPtr->blockSize);
						}

						return blockRef;
					}
				}
			}

			var newBlockRef = AllocateBlock(requiredBlockSize, out var newBlockPtr);

			var newBlock = *newBlockPtr;
			MemoryExt.MemCopy((byte*)blockPtr, (byte*)newBlockPtr, blockPtr->blockSize);
			*newBlockPtr = newBlock;

			FreeBlock(blockRef);

			blockPtr = newBlockPtr;
			return newBlockRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FreeBlock(MemoryBlockRef blockRef)
		{
			E.ASSERT(blockRef.memoryZoneId >= 0 && blockRef.memoryZoneId < zonesList.count);

			ref var zone = ref zonesList[blockRef.memoryZoneId];
			E.ASSERT(zone.size > blockRef.memoryZoneOffset);

			var blockPtr = (MemoryBlock*)(zone.memory + blockRef.memoryZoneOffset);
			E.ASSERT(zone.size >= blockRef.memoryZoneOffset + blockPtr->blockSize);

			// Если предыдущий блок пустой, то мерджим предыдущий блок
			var prevBlockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->prevBlockOffset);
			if (prevBlockPtr->id.freeId >= 0)
			{
				prevBlockPtr->blockSize += blockPtr->blockSize;
				RemoveFreeBlock(blockPtr->id);

				// Если следующий блок существует и он пустой, то мерджим следующий блок
				if (zone.size > blockRef.memoryZoneOffset + blockPtr->blockSize)
				{
					var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->blockSize);
					if (nextBlockPtr->id.freeId >= 0)
					{
						prevBlockPtr->blockSize += nextBlockPtr->blockSize;
						RemoveFreeBlock(nextBlockPtr->id);
					}
				}

				// Если размер изменился, то перемещаем ссылку на блок в другую коллекцию
				var newBlockSizeId = prevBlockPtr->blockSize.Log2() - MIN_BLOCK_SIZE_LOG2;
				if (prevBlockPtr->id.sizeId != newBlockSizeId)
				{
					var prevBlockRef = RemoveFreeBlock(prevBlockPtr->id);
					prevBlockPtr->id.sizeId = newBlockSizeId;

					AddFreeBlock(prevBlockPtr, prevBlockRef);
				}
			}
			else
			{
				// Если следующий блок существует и он пустой, то мерджим следующий блок
				if (zone.size > blockRef.memoryZoneOffset + blockPtr->blockSize)
				{
					var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->blockSize);
					if (nextBlockPtr->id.freeId >= 0)
					{
						RemoveFreeBlock(nextBlockPtr->id);

						// Если размер изменился, то меняем sizeId
						blockPtr->blockSize += nextBlockPtr->blockSize;
						blockPtr->id.sizeId = blockPtr->blockSize.Log2() - MIN_BLOCK_SIZE_LOG2;
					}
				}

				// Восстанавливаем блок как свободный
				AddFreeBlock(blockPtr, blockRef);
			}
		}
	}
}
