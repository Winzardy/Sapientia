using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxy(typeof(AllocatorProxy))]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe partial struct Allocator : IDisposable
	{
		public AllocatorId allocatorId;

		private UnsafeList<MemoryZone> zonesList;
		private UnsafeList<MemoryBlockPtrCollection> freeBlocksCollections;

		public ServiceRegistry serviceRegistry;

		public ushort version;

		public int ZoneSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => zonesList[0]->size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Initialize(AllocatorId allocatorId, int zoneSize = MIN_ZONE_SIZE)
		{
			this.allocatorId = allocatorId;
			this.version = 1;

			zoneSize = zoneSize.Max(MIN_ZONE_SIZE);

			zonesList = new UnsafeList<MemoryZone>();

			var maxBlockSizeId = GetBlockSizeId(zoneSize) + 1;
			freeBlocksCollections = new UnsafeList<MemoryBlockPtrCollection>(maxBlockSizeId);

			var blockSize = MIN_BLOCK_SIZE;
			for (var i = 0; i < freeBlocksCollections.capacity; i++)
			{
				freeBlocksCollections.Add(new MemoryBlockPtrCollection(blockSize));
				blockSize <<= 1;
			}

			AllocateMemoryZone(zoneSize);

			serviceRegistry = ServiceRegistry.Create(new SafePtr<Allocator>(this.AsPointer(), 1));
		}

		public void Dispose()
		{
			for (var i = 0; i < freeBlocksCollections.count; i++)
			{
				freeBlocksCollections[i]->Dispose();
			}
			freeBlocksCollections.Dispose();

			for (var i = 0; i < zonesList.count; i++)
			{
				zonesList[i]->Dispose();
			}
			zonesList.Dispose();

			this = default;
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
				size += zonesList[i]->size;
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
