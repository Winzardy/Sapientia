using System;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator.My
{
	public unsafe partial struct Allocator : IDisposable
	{
		public AllocatorId allocatorId;
		private UnsafeList<MemoryZone> zonesList;
		private UnsafeList<MemoryBlockPtrCollection> freeBlocksCollections;

		public ServiceRegistry serviceRegistry;

		public int ZoneSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => zonesList[0].size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Allocator Initialize(AllocatorId allocatorId, int zoneSize)
		{
			this.allocatorId = allocatorId;

			zoneSize = zoneSize.Max(MIN_ZONE_SIZE);

			zonesList = new UnsafeList<MemoryZone>();
			freeBlocksCollections = new UnsafeList<MemoryBlockPtrCollection>((zoneSize / MIN_BLOCK_SIZE).Log2());

			var blockSize = MIN_BLOCK_SIZE;
			for (var i = 0; i < freeBlocksCollections.capacity; i++)
			{
				freeBlocksCollections.Add(new MemoryBlockPtrCollection(blockSize));
				blockSize *= 2;
			}

			AllocateMemoryZone(zoneSize);

			//serviceRegistry = ServiceRegistry.Create((Allocator*)this.AsPointer());

			return this;
		}

		public void Dispose()
		{
			for (var i = 0; i < freeBlocksCollections.count; i++)
			{
				freeBlocksCollections[i].Dispose();
			}
			freeBlocksCollections.Dispose();

			for (var i = 0; i < zonesList.count; i++)
			{
				zonesList[i].Dispose();
			}
			zonesList.Dispose();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void GetSize(out int reservedSize, out int usedSize, out int freeSize)
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
		public readonly int GetReservedSize()
		{
			var size = 0;
			for (var i = 0; i < zonesList.count; i++)
			{
				size += zonesList[i].size;
			}

			return size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int GetUsedSize()
		{
			GetSize(out var reservedSize, out var usedSize, out var freeSize);
			return usedSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int GetFreeSize()
		{
			GetSize(out var reservedSize, out var usedSize, out var freeSize);
			return freeSize;
		}
	}
}
