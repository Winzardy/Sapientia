using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct MemoryBlockPtrCollection : IDisposable
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
			var blockPtr = (zonesList[memPtr.zoneId].ptr->memory + memPtr.zoneOffset - TSize<MemoryBlock>.size).Cast<MemoryBlock>();
			return blockPtr.ptr->blockSize;
		}

		private void CreateFreeBlock(SafePtr<MemoryZone> zone, SafePtr<MemoryBlock> blockPtr, MemoryBlockRef blockRef, int blockSize, int prevBlockOffset)
		{
			E.ASSERT(blockPtr.ptr < zone.ptr->zoneEnd);
			E.ASSERT(prevBlockOffset <= 0);

			var sizeId = GetBlockSizeId(blockSize);

			// Иногда возвращается блок больше 2^(sizeId + MIN_BLOCK_SIZE_LOG2)
			// Это возникает если размер зоны не степень двойки
			if (sizeId >= freeBlockPools.count)
				sizeId = freeBlockPools.count - 1;

			var sizeBlocksCollection = freeBlockPools[sizeId];

			E.ASSERT(sizeBlocksCollection.ptr->blockSize <= blockSize);

			// Создаём свободный блок из остатка памяти
			blockPtr.ptr->blockSize = blockSize;
			blockPtr.ptr->prevBlockOffset = prevBlockOffset;
			blockPtr.ptr->id = new BlockId(sizeId, sizeBlocksCollection.ptr->freeBlocks.count);

			// Добавляем ссылку на новый блок в список свободных блоков
			sizeBlocksCollection.ptr->freeBlocks.Add(blockRef);

			// Если существует следующий блок, то обновляем его ссылку на предыдущий блок
			var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr.ptr + blockPtr.ptr->blockSize);
			if (nextBlockPtr < zone.ptr->zoneEnd)
				nextBlockPtr->prevBlockOffset = -blockPtr.ptr->blockSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AddFreeBlock(SafePtr<MemoryBlock> blockPtr, MemoryBlockRef blockRef)
		{
			E.ASSERT(!blockPtr.ptr->id.IsFree);

			var freeBlocksCollection = freeBlockPools[blockPtr.ptr->id.sizeId];

			E.ASSERT(blockPtr.ptr->blockSize >= freeBlocksCollection.ptr->blockSize);

			ref var freeBlocks = ref freeBlocksCollection.ptr->freeBlocks;
			blockPtr.ptr->id.freeId = freeBlocks.count;

			freeBlocks.Add(blockRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef RemoveLastFreeBlock(int sizeId, out SafePtr<MemoryZone> zone, out SafePtr<MemoryBlock> blockPtr)
		{
			E.ASSERT(sizeId >= 0 && sizeId < freeBlockPools.count);
			E.ASSERT(freeBlockPools[sizeId].ptr->Count > 0);

			ref var freeBlocks = ref freeBlockPools[sizeId].ptr->freeBlocks;
			var blockRef = freeBlocks.RemoveLast();

			zone = zonesList[blockRef.memoryZoneId];
			blockPtr = (zone.ptr->memory + blockRef.memoryZoneOffset).Cast<MemoryBlock>();

			E.ASSERT(blockPtr.ptr->id.IsFree);
			E.ASSERT(blockPtr.ptr < zone.ptr->zoneEnd);

			blockPtr.ptr->id.freeId = -1;
			return blockRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef RemoveFreeBlock(ref BlockId blockId)
		{
			E.ASSERT(blockId.sizeId >= 0 && blockId.sizeId < freeBlockPools.count);
			E.ASSERT(blockId.IsFree);

			ref var freeBlocks = ref freeBlockPools[blockId.sizeId].ptr->freeBlocks;
			var blockRef = freeBlocks.RemoveAtSwapBack(blockId.freeId);

			// Если ссылка на блок удалена из середины, то меняем freeId блока этой ссылки
			if (freeBlocks.count > blockId.freeId)
			{
				var swappedBlockRef = freeBlocks[blockId.freeId];
				var zone = zonesList[swappedBlockRef.ptr->memoryZoneId];

				var swappedBlockPtr = (zone.ptr->memory + swappedBlockRef.ptr->memoryZoneOffset).Cast<MemoryBlock>();
				swappedBlockPtr.ptr->id.freeId = blockId.freeId;
			}

			blockId.freeId = -1;
			return blockRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef AllocateBlock(int requiredBlockSize, out SafePtr<MemoryBlock> blockPtr)
		{
			// `requiredSizeId` округлён вверх, т.к. хотим найти блок не меньше требуемого размера
			// `requiredBlockSizeId` - это индекс блока размером <= `requiredBlockSize`
			var requiredBlockSizeId = GetBlockSizeId(requiredBlockSize, out var requiredSizeId);

			E.ASSERT(requiredSizeId >= 0 && requiredSizeId < freeBlockPools.count);

			var blocksCollection = freeBlockPools[requiredSizeId];

			// Ищем свободный блок памяти, подходящий под требуемый размер
			while (blocksCollection.ptr->Count == 0)
			{
				requiredSizeId++;

				// Если такого блока нет, то аллоцируем ещё одну зону
				if (requiredSizeId >= freeBlockPools.count)
				{
					AllocateMemoryZone();
					// При аллокации зоны аллоцируется блок самого большого размера
					requiredSizeId = freeBlockPools.count - 1;
				}

				blocksCollection = freeBlockPools[requiredSizeId];
			}

			// Берём последний свободный блок
			var blockRef = RemoveLastFreeBlock(requiredSizeId, out var zone, out blockPtr);
			E.ASSERT(blockPtr.ptr->id.sizeId == requiredSizeId);

			// Если у блока осталось свободное место, то создаём новый свободный блок
			var extraSize = blockPtr.ptr->blockSize - requiredBlockSize;
			E.ASSERT(extraSize >= 0);
			if (extraSize >= MIN_BLOCK_SIZE)
			{
				var extraBlockPtr = ((SafePtr)blockPtr + requiredBlockSize).Cast<MemoryBlock>();
				var extraBlockRef = new MemoryBlockRef(blockRef.memoryZoneId, blockRef.memoryZoneOffset + requiredBlockSize);

				CreateFreeBlock(zone, extraBlockPtr, extraBlockRef, extraSize, -requiredBlockSize);

				// Устанавливаем требуемый размер блока
				blockPtr.ptr->blockSize = requiredBlockSize;
				blockPtr.ptr->id.sizeId = requiredBlockSizeId;
			}

			return blockRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MemoryBlockRef ReAllocateBlock(MemoryBlockRef blockRef, int requiredBlockSize, out SafePtr<MemoryBlock> blockPtr)
		{
			var zone = zonesList[blockRef.memoryZoneId];
			blockPtr = (zone.ptr->memory + blockRef.memoryZoneOffset).Cast<MemoryBlock>();

			// Если в тукущем блоке достаточно памяти, то ничего не делаем
			if (blockPtr.ptr->blockSize >= requiredBlockSize)
				return blockRef;

			E.ASSERT(blockPtr.ptr->blockSize > 0);

			// TODO: Добавить обработку предыдущего блока
			var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr.ptr + blockPtr.ptr->blockSize);
			if (nextBlockPtr < zone.ptr->zoneEnd && nextBlockPtr->id.IsFree)
			{
				var requiredSize = requiredBlockSize - blockPtr.ptr->blockSize;
				var extraSize = nextBlockPtr->blockSize - requiredSize;

				// Если в следующем блоке достаточно памяти, то увеличиваем размер текущего блока
				if (extraSize >= 0)
				{
					RemoveFreeBlock(ref nextBlockPtr->id);

					blockPtr.ptr->blockSize += requiredSize;

					// Если остаток меньше минимального размера блока, то оставляем остаток памяти в текущем блоке
					if (extraSize < MIN_BLOCK_SIZE)
					{
						blockPtr.ptr->blockSize += extraSize;

						// Если следующий блок существует, то обновляем ссылку на предыдущий блок
						nextBlockPtr = (MemoryBlock*)((byte*)nextBlockPtr + nextBlockPtr->blockSize);
						E.ASSERT(!nextBlockPtr->id.IsFree);

						if (nextBlockPtr < zone.ptr->zoneEnd)
							nextBlockPtr->prevBlockOffset = -blockPtr.ptr->blockSize;
					}
					else
					{
						var extraBlockPtr = ((SafePtr)blockPtr + requiredBlockSize).Cast<MemoryBlock>();
						var extraBlockRef = new MemoryBlockRef(blockRef.memoryZoneId, blockRef.memoryZoneOffset + blockPtr.ptr->blockSize);

						CreateFreeBlock(zone, extraBlockPtr, extraBlockRef, extraSize, -blockPtr.ptr->blockSize);
					}

					// Обновляем `sizeId` блока
					blockPtr.ptr->id.sizeId = GetBlockSizeId(blockPtr.ptr->blockSize);
					return blockRef;
				}
			}

			var newBlockRef = AllocateBlock(requiredBlockSize, out var newBlockPtr);

			// Копируем данные из старого блока в новый
			var newBlock = newBlockPtr.Value();
			MemoryExt.MemCopy((SafePtr)blockPtr, (SafePtr)newBlockPtr, blockPtr.ptr->blockSize);
			// Восстанавливаем данные нового блока
			newBlockPtr.Value() = newBlock;

			// Освобождаем старый блок
			FreeBlock(blockRef);

			blockPtr = newBlockPtr;
			return newBlockRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FreeBlock(MemoryBlockRef blockRef)
		{
			var zone = zonesList[blockRef.memoryZoneId];
			var blockPtr = (zone.ptr->memory + blockRef.memoryZoneOffset).Cast<MemoryBlock>();

			E.ASSERT(blockRef.memoryZoneId >= 0 && blockRef.memoryZoneId < zonesList.count);
			E.ASSERT(zone.ptr->size >= blockRef.memoryZoneOffset + blockPtr.ptr->blockSize);
			E.ASSERT(!blockPtr.ptr->id.IsFree);

			// Если предыдущий блок существует и свободен, то мерджим предыдущий блок
			var prevBlockPtr = ((SafePtr)blockPtr + blockPtr.ptr->prevBlockOffset).Cast<MemoryBlock>();
			if (prevBlockPtr != blockPtr && prevBlockPtr.ptr->id.IsFree)
			{
				E.ASSERT(prevBlockPtr.ptr >= zone.ptr->memory.ptr);

				prevBlockPtr.ptr->blockSize += blockPtr.ptr->blockSize;
				E.ASSERT((SafePtr)blockPtr + blockPtr.ptr->blockSize == (SafePtr)prevBlockPtr + prevBlockPtr.ptr->blockSize);

				// Если следующий блок существует и он пустой, то мерджим следующий блок
				var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr.ptr + blockPtr.ptr->blockSize);
				if (nextBlockPtr < zone.ptr->zoneEnd && nextBlockPtr->id.IsFree)
				{
					prevBlockPtr.ptr->blockSize += nextBlockPtr->blockSize;
					RemoveFreeBlock(ref nextBlockPtr->id);
				}

				// Если размер изменился, то перемещаем ссылку на блок в другую коллекцию
				var newBlockSizeId = GetBlockSizeId(prevBlockPtr.ptr->blockSize);
				if (prevBlockPtr.ptr->id.sizeId != newBlockSizeId)
				{
					var prevBlockRef = RemoveFreeBlock(ref prevBlockPtr.ptr->id);
					prevBlockPtr.ptr->id.sizeId = newBlockSizeId;

					AddFreeBlock(prevBlockPtr, prevBlockRef);
				}
			}
			else
			{
				// Если следующий блок существует и он пустой, то мерджим следующий блок
				var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr.ptr + blockPtr.ptr->blockSize);
				if (nextBlockPtr < zone.ptr->zoneEnd && nextBlockPtr->id.IsFree)
				{
					E.ASSERT(zone.ptr->size > blockRef.memoryZoneOffset + blockPtr.ptr->blockSize);

					blockPtr.ptr->blockSize += nextBlockPtr->blockSize;
					// Обновляем sizeId прежде чем добавлять блок в список свободных
					blockPtr.ptr->id.sizeId = GetBlockSizeId(blockPtr.ptr->blockSize);

					RemoveFreeBlock(ref nextBlockPtr->id);
				}

				// Восстанавливаем блок как свободный
				AddFreeBlock(blockPtr, blockRef);
			}
		}
	}
}
