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
		public readonly struct MemoryZone : IDisposable
		{
			public readonly SafePtr memory;
			public readonly byte* zoneEnd;
			public readonly int size;

			public MemoryZone(MemoryBlock firstMemoryBlock, int size)
			{
				this.memory = MemoryExt.MemAlloc(size);
				this.zoneEnd = memory.ptr + size;
				this.size = size;

				memory.Value<MemoryBlock>() = firstMemoryBlock;
			}

			public MemoryZone(SafePtr memory, MemoryBlock firstMemoryBlock, int size)
			{
				this.memory = memory;
				this.zoneEnd = memory.ptr + size;
				this.size = size;

				memory.Value<MemoryBlock>() = firstMemoryBlock;
			}

			public void Dispose()
			{
				MemoryExt.MemFree(memory);
			}
		}

		private readonly int GetMemoryZoneSize(int zoneId, out int usedSize, out int freeSize)
		{
			var zone = zonesList[zoneId];
			var blockPtr = zone.ptr->memory.Cast<MemoryBlock>();
			var totalSize = 0;

			freeSize = 0;
			do
			{
				if (blockPtr.ptr->id.freeId >= 0)
					freeSize += blockPtr.ptr->blockSize;
				totalSize += blockPtr.ptr->blockSize;

				blockPtr = ((SafePtr)blockPtr + blockPtr.ptr->blockSize);
			} while (totalSize < zone.ptr->size);

			usedSize = zone.ptr->size - freeSize;

			return zone.ptr->size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AllocateMemoryZone()
		{
			AllocateMemoryZone(ZoneSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AllocateMemoryZone(int zoneSize)
		{
			var blockId = new BlockId(freeBlockPools.count - 1, freeBlockPools[freeBlockPools.count - 1].ptr->Count);
			var memoryBlock = MemoryBlock.CreateFirstBlock(blockId, zoneSize);
			var zone = new MemoryZone(memoryBlock, zoneSize);

			freeBlockPools[freeBlockPools.count - 1].ptr->AddBlock(new MemoryBlockRef(zonesList.count, 0));
			zonesList.Add(zone);

#if UNITY_5_3_OR_NEWER
			UnityEngine.Debug.LogWarning($"Zone allocated with Size: {zoneSize}");
#endif
		}

		private static void MemoryZoneDumpHeap(SafePtr<MemoryZone> zone, SimpleList<string> results)
		{
			results.Add($"zone size: {zone.ptr->size}; location: {new IntPtr(zone.ptr)};");

			var blockPtr = zone.ptr->memory.Cast<MemoryBlock>();
			while (blockPtr.ptr->blockSize > 0)
			{
				results.Add($"block offset: {(byte*)blockPtr.ptr - zone.ptr->memory.ptr}; freeId: {blockPtr.ptr->id.freeId}; sizeId: {blockPtr.ptr->id.sizeId}; size: {blockPtr.ptr->blockSize}; prevBlockOffset: {blockPtr.ptr->prevBlockOffset};");
				blockPtr++;
			}
		}
	}
}
