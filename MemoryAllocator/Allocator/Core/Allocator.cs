using System;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator : IDisposable
	{
		private UnsafeList<MemoryZone> _zonesList;
		private UnsafeList<MemoryBlockPtrCollection> _freeBlockPools;

		public int ZoneSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _zonesList[0].ptr->size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Initialize(int zoneSize = MIN_ZONE_SIZE)
		{
			zoneSize = zoneSize.Max(MIN_ZONE_SIZE);

			_zonesList = new UnsafeList<MemoryZone>();

			var maxBlockSizeId = GetBlockSizeId(zoneSize) + 1;
			_freeBlockPools = new UnsafeList<MemoryBlockPtrCollection>(maxBlockSizeId);

			var blockSize = MIN_BLOCK_SIZE;
			for (var i = 0; i < _freeBlockPools.capacity; i++)
			{
				_freeBlockPools.Add(new MemoryBlockPtrCollection(blockSize));
				blockSize <<= 1;
			}

			AllocateMemoryZone(zoneSize);
		}

		public void Reset()
		{
			for (var i = 0; i < _freeBlockPools.count; i++)
			{
				_freeBlockPools[i].Value().Reset();
			}

			for (var i = 0; i < _zonesList.count; i++)
			{
				ref var pool = ref _freeBlockPools[_freeBlockPools.count - 1].Value();
				ref var zone = ref _zonesList[i].Value();
				var blockId = new BlockId(_freeBlockPools.count - 1, pool.Count);
				var memoryBlock = MemoryBlock.CreateFirstBlock(blockId, zone.size);

				// Reset first block
				zone.memory.Cast<MemoryBlock>().Value() = memoryBlock;
				pool.AddBlock(new MemoryBlockRef(i, 0));
			}
		}

		public void Dispose()
		{
			for (var i = 0; i < _freeBlockPools.count; i++)
			{
				_freeBlockPools[i].ptr->Dispose();
			}
			_freeBlockPools.Dispose();

			for (var i = 0; i < _zonesList.count; i++)
			{
				_zonesList[i].ptr->Dispose();
			}
			_zonesList.Dispose();

			_freeBlockPools = default;
			_zonesList = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void GetSize(out int reservedSize, out int usedSize, out int freeSize)
		{
			usedSize = 0;
			reservedSize = 0;
			freeSize = 0;
			for (var i = 0; i < _zonesList.count; i++)
			{
				reservedSize += GetMemoryZoneSize(i, out var usedZoneSize, out var freeZoneSize);
				usedSize += usedZoneSize;
				freeSize += freeZoneSize;
			}

			freeSize = reservedSize - usedSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetReservedSize()
		{
			var size = 0;
			for (var i = 0; i < _zonesList.count; i++)
			{
				size += _zonesList[i].ptr->size;
			}

			return size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetUsedSize()
		{
			GetSize(out var reservedSize, out var usedSize, out var freeSize);
			return usedSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetFreeSize()
		{
			GetSize(out var reservedSize, out var usedSize, out var freeSize);
			return freeSize;
		}
	}
}
