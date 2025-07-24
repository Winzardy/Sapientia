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
			private UnsafeList<MemoryBlockRef> _freeBlocks;

#if DEBUG
			public int allocatedCount;
			public int maxAllocatedCount;
#endif

			public int Count
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _freeBlocks.count;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => _freeBlocks.count = value;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public MemoryBlockPtrCollection(int blockSize, int capacity = 8)
			{
				this = default;
				this.blockSize = blockSize;
				this._freeBlocks = new UnsafeList<MemoryBlockRef>(capacity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public SafePtr<MemoryBlockRef> GetInnerArray()
			{
				return _freeBlocks.ptr;
			}

			public ref MemoryBlockRef this[int index]
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => ref _freeBlocks[index];
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void AddBlock(in MemoryBlockRef blockRef)
			{
#if DEBUG
				DecrementAllocatedCount();
#endif
				_freeBlocks.Add(blockRef);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public MemoryBlockRef RemoveAtSwapBackBlock(int index)
			{
#if DEBUG
				IncrementAllocatedCount();
#endif
				return _freeBlocks.RemoveAtSwapBack(index);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public MemoryBlockRef RemoveLastBlock()
			{
#if DEBUG
				IncrementAllocatedCount();
#endif
				return _freeBlocks.RemoveLast();
			}

#if DEBUG
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void IncrementAllocatedCount()
			{
				allocatedCount++;
#if UNITY_5_3_OR_NEWER
				if (allocatedCount > maxAllocatedCount)
				{
					maxAllocatedCount = allocatedCount;
					if (maxAllocatedCount % 1000 == 0)
					{
						UnityEngine.Debug.LogWarning($"Allocated {maxAllocatedCount} blocks of size {blockSize}.");
					}
				}
#endif
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void DecrementAllocatedCount()
			{
				allocatedCount--;
			}
#endif

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				_freeBlocks.Clear();
#if DEBUG
				allocatedCount = 0;
				maxAllocatedCount = 0;
#endif
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				_freeBlocks.Dispose();
				this = default;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetBlockSize(in MemPtr memPtr)
		{
			var blockPtr = (_zonesList[memPtr.zoneId].memory + memPtr.zoneOffset - TSize<MemoryBlock>.size).Cast<MemoryBlock>();
			return blockPtr.ptr->blockSize;
		}

		private void CreateFreeBlock(SafePtr<MemoryZone> zone, SafePtr<MemoryBlock> blockPtr, MemoryBlockRef blockRef, int requiredBlockSize, int prevBlockOffset)
		{
			E.ASSERT(blockPtr.ptr >= zone.ptr->memory.ptr && blockPtr.ptr < zone.ptr->zoneEnd, $"{nameof(CreateFreeBlock)}. Указатель на блок находится вне выделенной области памяти.");
			E.ASSERT(prevBlockOffset <= 0, $"{nameof(CreateFreeBlock)}. Смещение к предыдущему блоку не корректно.");

			var sizeId = GetBlockSizeId(requiredBlockSize);

			// Иногда возвращается блок больше 2^(sizeId + MIN_BLOCK_SIZE_LOG2)
			// Это возникает если размер зоны не степень двойки
			if (sizeId >= _freeBlockPools.count)
				sizeId = _freeBlockPools.count - 1;

			var sizeBlocksCollection = _freeBlockPools.ptr.Slice(sizeId);

			E.ASSERT(sizeBlocksCollection.ptr->blockSize <= requiredBlockSize, $"{nameof(CreateFreeBlock)}. Размер блоков коллекции меньше требуемого размера.");

			// Создаём свободный блок из остатка памяти
			blockPtr.ptr->blockSize = requiredBlockSize;
			blockPtr.ptr->prevBlockOffset = prevBlockOffset;
			blockPtr.ptr->id = new BlockId(sizeId, sizeBlocksCollection.ptr->Count);

			// Добавляем ссылку на новый блок в список свободных блоков
			sizeBlocksCollection.ptr->AddBlock(blockRef);

			// Если существует следующий блок, то обновляем его ссылку на предыдущий блок
			var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr.ptr + blockPtr.ptr->blockSize);
			if (nextBlockPtr < zone.ptr->zoneEnd)
				nextBlockPtr->prevBlockOffset = -blockPtr.ptr->blockSize;
		}

		private void AddFreeBlock(SafePtr<MemoryBlock> blockPtr, MemoryBlockRef blockRef)
		{
			E.ASSERT(!blockPtr.ptr->id.IsFree, $"{nameof(AddFreeBlock)}. Происходит попытка освободить уже свободный блок.");

			var freeBlocksCollection = _freeBlockPools.ptr.Slice(blockPtr.ptr->id.sizeId);

			E.ASSERT(blockPtr.ptr->blockSize >= freeBlocksCollection.ptr->blockSize, $"{nameof(AddFreeBlock)}. Размер блока больше, чем размер блоков коллекции.");

			ref var freeBlocks = ref freeBlocksCollection.Value();
			blockPtr.ptr->id.freeId = freeBlocks.Count;

			freeBlocks.AddBlock(blockRef);
		}

		private MemoryBlockRef RemoveLastFreeBlock(int sizeId, out SafePtr<MemoryZone> zone, out SafePtr<MemoryBlock> blockPtr)
		{
			E.ASSERT(sizeId >= 0 && sizeId < _freeBlockPools.count, $"{nameof(RemoveLastFreeBlock)}. Размер блока находится вне допустимых границ.");
			E.ASSERT(_freeBlockPools[sizeId].Count > 0, $"{nameof(RemoveLastFreeBlock)}. Попытка получить свободный блок из пустого пула.");

			var blockRef = _freeBlockPools[sizeId].RemoveLastBlock();

			zone = _zonesList.ptr.Slice(blockRef.memoryZoneId);
			blockPtr = (zone.ptr->memory + blockRef.memoryZoneOffset).Cast<MemoryBlock>();

			E.ASSERT(blockPtr.ptr->id.IsFree, $"{nameof(RemoveLastFreeBlock)}. При попытке получить свободный блок из пула был получен занятый блок.");
			E.ASSERT(blockPtr.ptr >= zone.ptr->memory.ptr && blockPtr.ptr < zone.ptr->zoneEnd, $"{nameof(RemoveLastFreeBlock)}. Указатель на блок находится вне выделенной области памяти.");

			blockPtr.ptr->id.freeId = -1;
#if DEBUG
			_freeBlockPools[sizeId].DecrementAllocatedCount();
#endif
			return blockRef;
		}

		private MemoryBlockRef RemoveFreeBlock(ref BlockId blockId)
		{
			E.ASSERT(blockId.sizeId >= 0 && blockId.sizeId < _freeBlockPools.count, $"{nameof(RemoveFreeBlock)}. Размер блока находится вне допустимых границ.");
			E.ASSERT(blockId.IsFree, $"{nameof(RemoveFreeBlock)}. Свободный блок оказался занятым.");

			ref var pool = ref _freeBlockPools[blockId.sizeId];
			var blockRef = pool.RemoveAtSwapBackBlock(blockId.freeId);

			// Если ссылка на блок удалена из середины, то меняем freeId блока этой ссылки
			if (pool.Count > blockId.freeId)
			{
				ref var swappedBlockRef = ref pool[blockId.freeId];
				var zone = _zonesList.ptr.Slice(swappedBlockRef.memoryZoneId);

				var swappedBlockPtr = (zone.ptr->memory + swappedBlockRef.memoryZoneOffset).Cast<MemoryBlock>();
				swappedBlockPtr.ptr->id.freeId = blockId.freeId;
			}

			blockId.freeId = -1;
			return blockRef;
		}

		private MemoryBlockRef AllocateBlock(int requiredBlockSize, out SafePtr<MemoryBlock> blockPtr)
		{
			// `requiredSizeId` округлён вверх, т.к. хотим найти блок не меньше требуемого размера
			// `requiredBlockSizeId` - это индекс блока размером <= `requiredBlockSize`
			var requiredBlockSizeId = GetBlockSizeId(requiredBlockSize, out var requiredSizeId);

			E.ASSERT(requiredSizeId >= 0 && requiredSizeId < _freeBlockPools.count, $"{nameof(AllocateBlock)}. Размер блока находится вне допустимых границ.");

			var blocksCollection = _freeBlockPools.ptr.Slice(requiredSizeId);

			// Ищем свободный блок памяти, подходящий под требуемый размер
			while (blocksCollection.ptr->Count == 0)
			{
				requiredSizeId++;

				// Если такого блока нет, то аллоцируем ещё одну зону
				if (requiredSizeId >= _freeBlockPools.count)
				{
					AllocateMemoryZone();
					// При аллокации зоны аллоцируется блок самого большого размера
					requiredSizeId = _freeBlockPools.count - 1;
				}

				blocksCollection = _freeBlockPools.ptr.Slice(requiredSizeId);
			}

			// Берём последний свободный блок
			var blockRef = RemoveLastFreeBlock(requiredSizeId, out var zone, out blockPtr);
			E.ASSERT(blockPtr.ptr->id.sizeId == requiredSizeId, $"{nameof(AllocateBlock)}. Был получен блок некорректного размера.");

			// Если у блока осталось свободное место, то создаём новый свободный блок
			var extraSize = blockPtr.ptr->blockSize - requiredBlockSize;
			E.ASSERT(extraSize >= 0, $"{nameof(AllocateBlock)}. Размер блока < 0.");
			if (extraSize >= MIN_BLOCK_SIZE)
			{
				var extraBlockPtr = ((SafePtr)blockPtr + requiredBlockSize).Cast<MemoryBlock>();
				var extraBlockRef = new MemoryBlockRef(blockRef.memoryZoneId, blockRef.memoryZoneOffset + requiredBlockSize);

				CreateFreeBlock(zone, extraBlockPtr, extraBlockRef, extraSize, -requiredBlockSize);

				// Устанавливаем требуемый размер блока
				blockPtr.ptr->blockSize = requiredBlockSize;
				blockPtr.ptr->id.sizeId = requiredBlockSizeId;
			}

#if DEBUG
			_freeBlockPools[blockPtr.ptr->id.sizeId].IncrementAllocatedCount();
#endif
			return blockRef;
		}

		private MemoryBlockRef ReAllocateBlock(MemoryBlockRef blockRef, int requiredBlockSize, out SafePtr<MemoryBlock> blockPtr)
		{
			var zone = _zonesList.ptr.Slice(blockRef.memoryZoneId);
			blockPtr = (zone.ptr->memory + blockRef.memoryZoneOffset).Cast<MemoryBlock>();

			// Если в тукущем блоке достаточно памяти, то ничего не делаем
			if (blockPtr.ptr->blockSize >= requiredBlockSize)
				return blockRef;

			E.ASSERT(blockPtr.ptr->blockSize > 0, $"{nameof(ReAllocateBlock)}. Размер блока <= 0");

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
						// Если сработало, то вероятно где-то происходит некорректное освобождение блока
						E.ASSERT(!nextBlockPtr->id.IsFree, $"{nameof(ReAllocateBlock)}. Следующий блок свободен, но этого быть не должно");

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

		private void FreeBlock(MemoryBlockRef blockRef)
		{
			ref var zone = ref _zonesList[blockRef.memoryZoneId];
			var blockPtr = (zone.memory + blockRef.memoryZoneOffset).Cast<MemoryBlock>();

			E.ASSERT(blockRef.memoryZoneId >= 0 && blockRef.memoryZoneId < _zonesList.count, $"{nameof(FreeBlock)}. Попытка вернуть блок в не существующую область памяти.");
			E.ASSERT(blockRef.memoryZoneOffset > 0 && zone.size >= blockRef.memoryZoneOffset + blockPtr.ptr->blockSize, $"{nameof(FreeBlock)}. Блок выходит за пределы области памяти.");
			E.ASSERT(!blockPtr.ptr->id.IsFree, $"{nameof(FreeBlock)}. Попытка освободить уже свободный блок.");

			// Если предыдущий блок существует и свободен, то мерджим предыдущий блок
			var prevBlockPtr = ((SafePtr)blockPtr + blockPtr.ptr->prevBlockOffset).Cast<MemoryBlock>();
			if (prevBlockPtr != blockPtr && prevBlockPtr.ptr->id.IsFree)
			{
				E.ASSERT(prevBlockPtr.ptr >= zone.memory.ptr && prevBlockPtr.ptr < zone.zoneEnd , $"{nameof(FreeBlock)}. Предыдущий блок выходит за пределы области памяти.");

				E.ASSERT(prevBlockPtr.ptr->blockSize == -blockPtr.ptr->prevBlockOffset, $"{nameof(FreeBlock)}. Указатель на предыдущий блок оказался не корректным.");
				prevBlockPtr.ptr->blockSize += blockPtr.ptr->blockSize;
				E.ASSERT((SafePtr)blockPtr + blockPtr.ptr->blockSize == (SafePtr)prevBlockPtr + prevBlockPtr.ptr->blockSize, $"{nameof(FreeBlock)}. Новый размер блока оказался не корректным.");

				// Если следующий блок существует и он пустой, то мерджим следующий блок
				var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr.ptr + blockPtr.ptr->blockSize);
				if (nextBlockPtr < zone.zoneEnd && nextBlockPtr->id.IsFree)
				{
					prevBlockPtr.ptr->blockSize += nextBlockPtr->blockSize;
					RemoveFreeBlock(ref nextBlockPtr->id);

					nextBlockPtr = (MemoryBlock*)((byte*)prevBlockPtr.ptr + prevBlockPtr.ptr->blockSize);
				}

				// Если размер изменился, то перемещаем ссылку на блок в другую коллекцию
				var newBlockSizeId = GetBlockSizeId(prevBlockPtr.ptr->blockSize);
				if (prevBlockPtr.ptr->id.sizeId != newBlockSizeId)
				{
					var prevBlockRef = RemoveFreeBlock(ref prevBlockPtr.ptr->id);
					prevBlockPtr.ptr->id.sizeId = newBlockSizeId;

					AddFreeBlock(prevBlockPtr, prevBlockRef);
				}

				nextBlockPtr->prevBlockOffset = -prevBlockPtr.ptr->blockSize;
			}
			else
			{
				// Если следующий блок существует и он пустой, то мерджим следующий блок
				var nextBlockPtr = (MemoryBlock*)((byte*)blockPtr.ptr + blockPtr.ptr->blockSize);
				if (nextBlockPtr < zone.zoneEnd && nextBlockPtr->id.IsFree)
				{
					blockPtr.ptr->blockSize += nextBlockPtr->blockSize;
					// Обновляем sizeId прежде чем добавлять блок в список свободных
					blockPtr.ptr->id.sizeId = GetBlockSizeId(blockPtr.ptr->blockSize);

					RemoveFreeBlock(ref nextBlockPtr->id);

					nextBlockPtr = (MemoryBlock*)((byte*)blockPtr.ptr + blockPtr.ptr->blockSize);
					nextBlockPtr->prevBlockOffset = -blockPtr.ptr->blockSize;
				}

				// Восстанавливаем блок как свободный
				AddFreeBlock(blockPtr, blockRef);
			}
		}
	}
}
