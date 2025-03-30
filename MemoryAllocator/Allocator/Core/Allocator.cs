using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Collections;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Collections;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxy(typeof(AllocatorProxy))]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe partial struct Allocator : IDisposable
	{
		public AllocatorId allocatorId;

		private UnsafeList<MemoryZone> zonesList;
		private UnsafeList<MemoryBlockPtrCollection> freeBlockPools;

		public ServiceRegistry serviceRegistry;

		public ushort version;

		public int ZoneSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => zonesList[0].ptr->size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Initialize(AllocatorId allocatorId, int zoneSize = MIN_ZONE_SIZE)
		{
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

			serviceRegistry = ServiceRegistry.Create(new SafePtr<Allocator>(this.AsPointer(), 1));
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
				size += zonesList[i].ptr->size;
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
