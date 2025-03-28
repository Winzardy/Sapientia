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
		private readonly struct MemoryZone : IDisposable
		{
			public readonly byte* memory;
			public readonly byte* zoneEnd;
			public readonly int size;

			public MemoryZone(MemoryBlock firstMemoryBlock, int size)
			{
				this.memory = (byte*)MemoryExt.MemAlloc(size);
				this.zoneEnd = memory + size;
				this.size = size;

				*((MemoryBlock*)memory) = firstMemoryBlock;
			}

			public void Dispose()
			{
				MemoryExt.MemFree(memory);
			}
		}

		private readonly int GetMemoryZoneSize(int zoneId, out int usedSize, out int freeSize)
		{
			var zone = zonesList[zoneId];
			var blockPtr = (MemoryBlock*)zone->memory;
			var totalSize = 0;

			freeSize = 0;
			do
			{
				if (blockPtr->id.freeId >= 0)
					freeSize += blockPtr->blockSize;
				totalSize += blockPtr->blockSize;

				blockPtr = (MemoryBlock*)((byte*)blockPtr + blockPtr->blockSize);
			} while (totalSize < zone->size);

			usedSize = zone->size - freeSize;

			return zone->size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AllocateMemoryZone()
		{
			AllocateMemoryZone(ZoneSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AllocateMemoryZone(int zoneSize)
		{
			var blockId = new BlockId(freeBlocksCollections.count - 1, freeBlocksCollections[freeBlocksCollections.count - 1]->Count);
			var memoryBlock = MemoryBlock.CreateFirstBlock(blockId, zoneSize);
			var zone = new MemoryZone(memoryBlock, zoneSize);

			freeBlocksCollections[freeBlocksCollections.count - 1]->freeBlocks.Add(new MemoryBlockRef(zonesList.count, 0));
			zonesList.Add(zone);

#if UNITY_5_3_OR_NEWER
			UnityEngine.Debug.LogWarning($"Zone allocated with Size: {zoneSize}");
#endif
		}

		private static void MemoryZoneDumpHeap(MemoryZone* zone, SimpleList<string> results)
		{
			results.Add($"zone size: {zone->size}; location: {new IntPtr(zone)};");

			var blockPtr = (MemoryBlock*)zone->memory;
			while (blockPtr->blockSize > 0)
			{
				results.Add($"block offset: {(byte*)blockPtr - zone->memory}; freeId: {blockPtr->id.freeId}; sizeId: {blockPtr->id.sizeId}; size: {blockPtr->blockSize}; prevBlockOffset: {blockPtr->prevBlockOffset};");
				blockPtr++;
			}
		}
	}
}
