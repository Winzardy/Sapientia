using Sapientia.Collections;
using Sapientia.Extensions;
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

				stream.Write(zone->size);
				E.ASSERT(zone->size > 0);

				stream.Write((byte*)zone->memory, zone->size);
			}

			// Записываем свободные блоки
			stream.Write(freeBlocksCollections.count);
			for (var i = 0; i < freeBlocksCollections.count; i++)
			{
				stream.Write(freeBlocksCollections[i]->blockSize);
				stream.Write(freeBlocksCollections[i]->freeBlocks.count);

				if (freeBlocksCollections[i]->freeBlocks.count == 0)
					continue;

				stream.Write(freeBlocksCollections[i]->freeBlocks.array, freeBlocksCollections[i]->freeBlocks.count);
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
				var zoneMemory = allocator.Value().zonesList[i]->memory;

				stream.Read(ref zoneMemory, zoneSize);
			}

			var blockCollectionsCount = stream.Read<int>();
			allocator.Value().freeBlocksCollections = new UnsafeList<MemoryBlockPtrCollection>(blockCollectionsCount);
			for (var i = 0; i < blockCollectionsCount; i++)
			{
				var blockSize = stream.Read<int>();
				var blocksCount = stream.Read<int>();
				allocator.Value().freeBlocksCollections.Add(new MemoryBlockPtrCollection(blockSize, blocksCount));

				if (blocksCount == 0)
					continue;

				ref var freeBlocks = ref allocator.Value().freeBlocksCollections[i]->freeBlocks;
				freeBlocks.count = blocksCount;
				stream.Read(ref freeBlocks.array, blocksCount);
			}

			return allocator;
		}
	}
}
