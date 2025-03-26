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
				ref var zone = ref zonesList[i];

				stream.Write(zone.size);
				E.ASSERT(zone.size > 0);

				stream.Write((byte*)zone.memory, zone.size);
			}

			// Записываем свободные блоки
			stream.Write(freeBlocksCollections.count);
			for (var i = 0; i < freeBlocksCollections.count; i++)
			{
				stream.Write(freeBlocksCollections[i].blockSize);
				stream.Write(freeBlocksCollections[i].freeBlocks.count);

				if (freeBlocksCollections[i].freeBlocks.count == 0)
					continue;

				stream.Write(freeBlocksCollections[i].freeBlocks.array, freeBlocksCollections[i].freeBlocks.count);
			}
		}

		public static Allocator* Deserialize(ref StreamBufferReader stream)
		{
			var allocator = MemoryExt.MemAlloc<Allocator>();

			stream.Read(ref allocator->allocatorId);
			stream.Read(ref allocator->serviceRegistry);
			stream.Read(ref allocator->version);

			allocator->version++;

			var zonesCount = stream.Read<int>();
			allocator->zonesList = new UnsafeList<MemoryZone>(zonesCount.Max(8));
			for (var i = 0; i < zonesCount; ++i)
			{
				var zoneSize = stream.Read<int>();
				E.ASSERT(zoneSize > 0);

				allocator->zonesList.Add(new MemoryZone(default, zoneSize));
				var zoneMemory = (byte*)allocator->zonesList[i].memory;

				stream.Read(ref zoneMemory, zoneSize);
			}


			var blockCollectionsCount = stream.Read<int>();
			allocator->freeBlocksCollections = new UnsafeList<MemoryBlockPtrCollection>(blockCollectionsCount);
			for (var i = 0; i < blockCollectionsCount; i++)
			{
				var blockSize = stream.Read<int>();
				var blocksCount = stream.Read<int>();
				allocator->freeBlocksCollections.Add(new MemoryBlockPtrCollection(blockSize, blocksCount));

				if (blocksCount == 0)
					continue;

				ref var freeBlocks = ref allocator->freeBlocksCollections[i].freeBlocks;
				freeBlocks.count = blocksCount;
				stream.Read(ref freeBlocks.array, blocksCount);
			}

			return allocator;
		}
	}
}
