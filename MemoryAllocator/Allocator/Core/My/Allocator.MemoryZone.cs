using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator.My
{
	public unsafe partial struct Allocator
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct MemoryZone : IDisposable
		{
			public MemoryBlock* memory;
			public readonly int size;

			public MemoryZone(MemoryBlock firstMemoryBlock, int size)
			{
				this.memory = (MemoryBlock*)MemoryExt.MemAlloc(size);
				this.size = size;

				*memory = firstMemoryBlock;
			}

			public void Dispose()
			{
				MemoryExt.MemFree(memory);
			}
		}

		private readonly int GetMemoryZoneSize(int zoneId, out int usedSize, out int freeSize)
		{
			ref var zone = ref zonesList[zoneId];
			var block = zone.memory;
			var totalSize = 0;

			freeSize = 0;
			do
			{
				if (block->id.freeId >= 0)
					freeSize += block->blockSize;
				totalSize += block->blockSize;

				block = block + block->blockSize;
			} while (totalSize < zone.size);

			usedSize = zone.size - freeSize;

			return zone.size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AllocateMemoryZone()
		{
			AllocateMemoryZone(ZoneSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AllocateMemoryZone(int zoneSize)
		{
			freeBlocksCollections[freeBlocksCollections.count - 1].Add(new MemoryBlockRef(0, 0));

			var blockId = new BlockId(freeBlocksCollections.count - 1, freeBlocksCollections[freeBlocksCollections.count - 1].Count);
			var memoryBlock = MemoryBlock.CreateFirstBlock(blockId, zoneSize);
			var zone = new MemoryZone(memoryBlock, zoneSize);
			zonesList.Add(zone);
		}
	}
}
