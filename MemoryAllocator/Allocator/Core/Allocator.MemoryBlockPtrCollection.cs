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
				this = default;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetBlockSize(in MemPtr memPtr)
		{
			var blockPtr = (MemoryBlock*)(zonesList[memPtr.zoneId]->memory + memPtr.zoneOffset - TSize<MemoryBlock>.size);
			return blockPtr->blockSize;
		}

		private void CreateFreeBlock(MemoryZone* zone, MemoryBlock* blockPtr, MemoryBlockRef blockRef, int blockSize, int prevBlockOffset)
		{
			E.ASSERT(blockPtr < zone->zoneEnd);
			E.ASSERT(prevBlockOffset <= 0);

			var sizeId = GetBlockSizeId(blockSize);

			// Иногда возвращается блок больше 2^(sizeId + MIN_BLOCK_SIZE_LOG2)
			// Это возникает если размер зоны не степень двойки
			if (sizeId >= freeBlocksCollections.count)
				sizeId = freeBlocksCollections.count - 1;

			var sizeBlocksCollection = freeBlocksCollections[sizeId];

			E.ASSERT(sizeBlocksCollection->blockSize <= blockSize);

			// Создаём свободный блок из остатка памяти
			blockPtr->blockSize = blockSize;
			blockPtr->prevBlockOffset = prevBlockOffset;
			blockPtr->id = new BlockId(sizeId, sizeBlocksCollection->freeBlocks.count);

			// Добавляем ссылку на новый блок в список свободных блоков
			sizeBlocksCollection->freeBlocks.Add(blockRef);

			// Если существует следующий блок, то обновляем его ссылку на предыдущий блок
			var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->blockSize);
			if (nextBlockPtr < zone->zoneEnd)
				nextBlockPtr->prevBlockOffset = -blockPtr->blockSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AddFreeBlock(MemoryBlock* blockPtr, MemoryBlockRef blockRef)
		{
			E.ASSERT(!blockPtr->id.IsFree);

			var freeBlocksCollection = freeBlocksCollections[blockPtr->id.sizeId];

			E.ASSERT(blockPtr->blockSize >= freeBlocksCollection->blockSize);

			ref var freeBlocks = ref freeBlocksCollection->freeBlocks;
			blockPtr->id.freeId = freeBlocks.count;

			freeBlocks.Add(blockRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef RemoveLastFreeBlock(int sizeId, out MemoryZone* zone, out MemoryBlock* blockPtr)
		{
			E.ASSERT(sizeId >= 0 && sizeId < freeBlocksCollections.count);
			E.ASSERT(freeBlocksCollections[sizeId]->Count > 0);

			ref var freeBlocks = ref freeBlocksCollections[sizeId]->freeBlocks;
			var blockRef = freeBlocks.RemoveLast();

			zone = zonesList[blockRef.memoryZoneId];
			blockPtr = (MemoryBlock*)(zone->memory + blockRef.memoryZoneOffset);

			E.ASSERT(blockPtr->id.IsFree);
			E.ASSERT(blockPtr < zone->zoneEnd);

			blockPtr->id.freeId = -1;
			return blockRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef RemoveFreeBlock(ref BlockId blockId)
		{
			E.ASSERT(blockId.sizeId >= 0 && blockId.sizeId < freeBlocksCollections.count);
			E.ASSERT(blockId.IsFree);

			ref var freeBlocks = ref freeBlocksCollections[blockId.sizeId]->freeBlocks;
			var blockRef = freeBlocks.RemoveAtSwapBack(blockId.freeId);

			// Если ссылка на блок удалена из середины, то меняем freeId блока этой ссылки
			if (freeBlocks.count > blockId.freeId)
			{
				var swappedBlockRef = freeBlocks[blockId.freeId];
				var zone = zonesList[swappedBlockRef->memoryZoneId];

				var swappedBlockPtr = (MemoryBlock*)(zone->memory + swappedBlockRef->memoryZoneOffset);
				swappedBlockPtr->id.freeId = blockId.freeId;
			}

			blockId.freeId = -1;
			return blockRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef AllocateBlock(int requiredBlockSize, out MemoryBlock* blockPtr)
		{
			// `requiredSizeId` округлён вверх, т.к. хотим найти блок не меньше требуемого размера
			// `requiredBlockSizeId` - это индекс блока размером <= `requiredBlockSize`
			var requiredBlockSizeId = GetBlockSizeId(requiredBlockSize, out var requiredSizeId);

			E.ASSERT(requiredSizeId >= 0 && requiredSizeId < freeBlocksCollections.count);

			var blocksCollection = freeBlocksCollections[requiredSizeId];

			// Ищем свободный блок памяти, подходящий под требуемый размер
			while (blocksCollection->Count == 0)
			{
				requiredSizeId++;

				// Если такого блока нет, то аллоцируем ещё одну зону
				if (requiredSizeId >= freeBlocksCollections.count)
				{
					AllocateMemoryZone();
					// При аллокации зоны аллоцируется блок самого большого размера
					requiredSizeId = freeBlocksCollections.count - 1;
				}

				blocksCollection = freeBlocksCollections[requiredSizeId];
			}

			// Берём последний свободный блок
			var blockRef = RemoveLastFreeBlock(requiredSizeId, out var zone, out blockPtr);
			E.ASSERT(blockPtr->id.sizeId == requiredSizeId);

			// Если у блока осталось свободное место, то создаём новый свободный блок
			var extraSize = blockPtr->blockSize - requiredBlockSize;
			E.ASSERT(extraSize >= 0);
			if (extraSize >= MIN_BLOCK_SIZE)
			{
				var extraBlockPtr = (MemoryBlock*)((byte*)blockPtr + requiredBlockSize);
				var extraBlockRef = new MemoryBlockRef(blockRef.memoryZoneId, blockRef.memoryZoneOffset + requiredBlockSize);

				CreateFreeBlock(zone, extraBlockPtr, extraBlockRef, extraSize, -requiredBlockSize);

				// Устанавливаем требуемый размер блока
				blockPtr->blockSize = requiredBlockSize;
				blockPtr->id.sizeId = requiredBlockSizeId;
			}

			return blockRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef ReAllocateBlock(MemoryBlockRef blockRef, int requiredBlockSize, out MemoryBlock* blockPtr)
		{
			var zone = zonesList[blockRef.memoryZoneId];
			blockPtr = (MemoryBlock*)(zone->memory + blockRef.memoryZoneOffset);

			// Если в тукущем блоке достаточно памяти, то ничего не делаем
			if (blockPtr->blockSize >= requiredBlockSize)
				return blockRef;

			E.ASSERT(blockPtr->blockSize > 0);

			// TODO: Добавить обработку предыдущего блока
			var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->blockSize);
			if (nextBlockPtr < zone->zoneEnd && nextBlockPtr->id.IsFree)
			{
				var requiredSize = requiredBlockSize - blockPtr->blockSize;
				var extraSize = nextBlockPtr->blockSize - requiredSize;

				// Если в следующем блоке достаточно памяти, то увеличиваем размер текущего блока
				if (extraSize >= 0)
				{
					RemoveFreeBlock(ref nextBlockPtr->id);

					blockPtr->blockSize += requiredSize;

					// Если остаток меньше минимального размера блока, то оставляем остаток памяти в текущем блоке
					if (extraSize < MIN_BLOCK_SIZE)
					{
						blockPtr->blockSize += extraSize;

						// Если следующий блок существует, то обновляем ссылку на предыдущий блок
						nextBlockPtr = (MemoryBlock*)((byte*)nextBlockPtr + nextBlockPtr->blockSize);
						E.ASSERT(!nextBlockPtr->id.IsFree);
						if (nextBlockPtr < zone->zoneEnd)
							nextBlockPtr->prevBlockOffset = -blockPtr->blockSize;
					}
					else
					{
						var extraBlockPtr = (MemoryBlock*)((byte*)blockPtr + requiredBlockSize);
						var extraBlockRef = new MemoryBlockRef(blockRef.memoryZoneId, blockRef.memoryZoneOffset + blockPtr->blockSize);

						CreateFreeBlock(zone, extraBlockPtr, extraBlockRef, extraSize, -blockPtr->blockSize);
					}

					// Обновляем `sizeId` блока
					blockPtr->id.sizeId = GetBlockSizeId(blockPtr->blockSize);
					return blockRef;
				}
			}

			var newBlockRef = AllocateBlock(requiredBlockSize, out var newBlockPtr);

			// Копируем данные из старого блока в новый
			var newBlock = *newBlockPtr;
			MemoryExt.MemCopy((byte*)blockPtr, (byte*)newBlockPtr, blockPtr->blockSize);
			// Восстанавливаем данные нового блока
			*newBlockPtr = newBlock;

			// Освобождаем старый блок
			FreeBlock(blockRef);

			blockPtr = newBlockPtr;
			return newBlockRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FreeBlock(MemoryBlockRef blockRef)
		{
			var zone = zonesList[blockRef.memoryZoneId];
			var blockPtr = (MemoryBlock*)(zone->memory + blockRef.memoryZoneOffset);

			E.ASSERT(blockRef.memoryZoneId >= 0 && blockRef.memoryZoneId < zonesList.count);
			E.ASSERT(zone->size >= blockRef.memoryZoneOffset + blockPtr->blockSize);
			E.ASSERT(!blockPtr->id.IsFree);

			// Если предыдущий блок существует и свободен, то мерджим предыдущий блок
			var prevBlockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->prevBlockOffset);
			if (prevBlockPtr != blockPtr && prevBlockPtr->id.IsFree)
			{
				E.ASSERT(prevBlockPtr >= zone->memory);

				prevBlockPtr->blockSize += blockPtr->blockSize;
				E.ASSERT((byte*)blockPtr + blockPtr->blockSize == (byte*)prevBlockPtr + prevBlockPtr->blockSize);

				// Если следующий блок существует и он пустой, то мерджим следующий блок
				var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->blockSize);
				if (nextBlockPtr < zone->zoneEnd && nextBlockPtr->id.IsFree)
				{
					prevBlockPtr->blockSize += nextBlockPtr->blockSize;
					RemoveFreeBlock(ref nextBlockPtr->id);
				}

				// Если размер изменился, то перемещаем ссылку на блок в другую коллекцию
				var newBlockSizeId = GetBlockSizeId(prevBlockPtr->blockSize);
				if (prevBlockPtr->id.sizeId != newBlockSizeId)
				{
					var prevBlockRef = RemoveFreeBlock(ref prevBlockPtr->id);
					prevBlockPtr->id.sizeId = newBlockSizeId;

					AddFreeBlock(prevBlockPtr, prevBlockRef);
				}
			}
			else
			{
				// Если следующий блок существует и он пустой, то мерджим следующий блок
				var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->blockSize);
				if (nextBlockPtr < zone->zoneEnd && nextBlockPtr->id.IsFree)
				{
					E.ASSERT(zone->size > blockRef.memoryZoneOffset + blockPtr->blockSize);

					blockPtr->blockSize += nextBlockPtr->blockSize;
					// Обновляем sizeId прежде чем добавлять блок в список свободных
					blockPtr->id.sizeId = GetBlockSizeId(blockPtr->blockSize);

					RemoveFreeBlock(ref nextBlockPtr->id);
				}

				// Восстанавливаем блок как свободный
				AddFreeBlock(blockPtr, blockRef);
			}
		}
	}
}
