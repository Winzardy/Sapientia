using Sapientia.Collections;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Collections;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		public readonly void Serialize(ref StreamBufferWriter stream)
		{
			stream.Write(allocatorId);
			stream.Write(serviceRegistry);
			stream.Write(version);

			// Записываем память
			stream.Write(zonesList.count);
			for (var i = 0; i < zonesList.count; ++i)
			{
				var zone = zonesList[i];

				stream.Write(zone.ptr->size);
				E.ASSERT(zone.ptr->size > 0);

				stream.Write((byte*)zone.ptr->memory, zone.ptr->size);
			}

			// Записываем свободные блоки
			stream.Write(freeBlockPools.count);
			for (var i = 0; i < freeBlockPools.count; i++)
			{
				stream.Write(freeBlockPools[i].ptr->blockSize);
				stream.Write(freeBlockPools[i].ptr->freeBlocks.count);

				if (freeBlockPools[i].ptr->freeBlocks.count == 0)
					continue;

				stream.Write(freeBlockPools[i].ptr->freeBlocks.array.ptr, freeBlockPools[i].ptr->freeBlocks.count);
			}
		}

		public static SafePtr<Allocator> Deserialize(ref StreamBufferReader stream)
		{
			var allocator = new SafePtr<Allocator>(MemoryExt.MemAlloc<Allocator>(), 1);

			stream.Read(ref allocator.Value().allocatorId);
			stream.Read(ref allocator.Value().serviceRegistry);
			stream.Read(ref allocator.Value().version);

			allocator.Value().version++;

			var zonesCount = stream.Read<int>();
			allocator.Value().zonesList = new UnsafeList<MemoryZone>(zonesCount.Max(8));
			for (var i = 0; i < zonesCount; ++i)
			{
				var zoneSize = stream.Read<int>();
				E.ASSERT(zoneSize > 0);

				allocator.Value().zonesList.Add(new MemoryZone(default, zoneSize));
				var zoneMemoryPtr = allocator.Value().zonesList[i].ptr->memory.ptr;

				stream.Read(ref zoneMemoryPtr, zoneSize);
			}

			var blockCollectionsCount = stream.Read<int>();
			ref var freeBlockPools = ref allocator.Value().freeBlockPools;
			freeBlockPools = new UnsafeList<MemoryBlockPtrCollection>(blockCollectionsCount);

			for (var i = 0; i < blockCollectionsCount; i++)
			{
				var blockSize = stream.Read<int>();
				var blocksCount = stream.Read<int>();
				freeBlockPools.Add(new MemoryBlockPtrCollection(blockSize, blocksCount));

				if (blocksCount == 0)
					continue;

				ref var freeBlocks = ref freeBlockPools[i].ptr->freeBlocks;
				freeBlocks.count = blocksCount;

				var arrayPtr = freeBlocks.array.ptr;
				stream.Read(ref arrayPtr, blocksCount);
			}

			return allocator;
		}
	}
}
