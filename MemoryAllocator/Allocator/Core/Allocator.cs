using System;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial class Allocator : IDisposable
	{
		public AllocatorId allocatorId;

		public UnsafeList<MemoryZone> zonesList;
		public UnsafeList<MemoryBlockPtrCollection> freeBlockPools;

		internal DataAccessor dataAccessor;

		public ushort version;

		public bool IsValid => version > 0;

		public int ZoneSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => zonesList[0].ptr->size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Initialize(AllocatorId allocatorId, int zoneSize = MIN_ZONE_SIZE)
		{
			E.ASSERT(!IsValid);

			this.allocatorId = allocatorId;
			this.version = 1;

			zoneSize = zoneSize.Max(MIN_ZONE_SIZE);

			zonesList = new UnsafeList<MemoryZone>();

			var maxBlockSizeId = GetBlockSizeId(zoneSize) + 1;
			freeBlockPools = new UnsafeList<MemoryBlockPtrCollection>(maxBlockSizeId);

			var blockSize = MIN_BLOCK_SIZE;
			for (var i = 0; i < freeBlockPools.capacity; i++)
			{
				freeBlockPools.Add(new MemoryBlockPtrCollection(blockSize));
				blockSize <<= 1;
			}

			AllocateMemoryZone(zoneSize);

			dataAccessor = DataAccessor.Create(this);
		}

		public void Reset(AllocatorId allocatorId)
		{
			E.ASSERT(IsValid);

			this.allocatorId = allocatorId;
			this.version++;

			for (var i = 0; i < freeBlockPools.count; i++)
			{
				freeBlockPools[i].Value().Reset();
			}

			for (var i = 0; i < zonesList.count; i++)
			{
				ref var pool = ref freeBlockPools[freeBlockPools.count - 1].Value();
				ref var zone = ref zonesList[i].Value();
				var blockId = new BlockId(freeBlockPools.count - 1, pool.Count);
				var memoryBlock = MemoryBlock.CreateFirstBlock(blockId, zone.size);

				// Reset first block
				zone.memory.Cast<MemoryBlock>().Value() = memoryBlock;
				pool.AddBlock(new MemoryBlockRef(i, 0));
			}

			dataAccessor = DataAccessor.Create(this);
		}

		public void Dispose()
		{
			for (var i = 0; i < freeBlockPools.count; i++)
			{
				freeBlockPools[i].ptr->Dispose();
			}
			freeBlockPools.Dispose();

			for (var i = 0; i < zonesList.count; i++)
			{
				zonesList[i].ptr->Dispose();
			}
			zonesList.Dispose();

			freeBlockPools = default;
			zonesList = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void GetSize(out int reservedSize, out int usedSize, out int freeSize)
		{
			usedSize = 0;
			reservedSize = 0;
			freeSize = 0;
			for (var i = 0; i < zonesList.count; i++)
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
			for (var i = 0; i < zonesList.count; i++)
			{
				size += zonesList[i].ptr->size;
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
