using Sapientia.Collections;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		public void Serialize(ref StreamBufferWriter stream)
		{
			// Записываем память
			stream.Write(_zonesList.count);
			for (var i = 0; i < _zonesList.count; ++i)
			{
				var zone = _zonesList.ptr.Slice(i);

				stream.Write(zone.ptr->size);
				E.ASSERT(zone.ptr->size > 0);

				stream.Write(zone.ptr->memory, zone.ptr->size);
			}

			// Записываем свободные блоки
			stream.Write(_freeBlockPools.count);
			for (var i = 0; i < _freeBlockPools.count; i++)
			{
				ref var pool = ref _freeBlockPools[i];
				stream.Write(pool.blockSize);
				stream.Write(pool.Count);

				if (pool.Count == 0)
					continue;

				stream.Write(pool.GetInnerArray(), pool.Count);
			}
		}

		public static Allocator Deserialize(ref StreamBufferReader stream)
		{
			var allocator = new Allocator();

			var zonesCount = stream.Read<int>();
			allocator._zonesList = new UnsafeList<MemoryZone>(zonesCount.Max(8));
			for (var i = 0; i < zonesCount; ++i)
			{
				var zoneSize = stream.Read<int>();
				E.ASSERT(zoneSize > 0);

				allocator._zonesList.Add(new MemoryZone(default, zoneSize));
				var zoneMemoryPtr = allocator._zonesList[i].memory;

				stream.Read(ref zoneMemoryPtr, zoneSize);
			}

			var blockCollectionsCount = stream.Read<int>();
			ref var freeBlockPools = ref allocator._freeBlockPools;
			freeBlockPools = new UnsafeList<MemoryBlockPtrCollection>(blockCollectionsCount);

			for (var i = 0; i < blockCollectionsCount; i++)
			{
				var blockSize = stream.Read<int>();
				var blocksCount = stream.Read<int>();
				freeBlockPools.Add(new MemoryBlockPtrCollection(blockSize, blocksCount));

				if (blocksCount == 0)
					continue;

				ref var freeBlocks = ref freeBlockPools[i];
				freeBlocks.Count = blocksCount;

				var arrayPtr = freeBlocks.GetInnerArray();
				stream.Read(ref arrayPtr, blocksCount);
			}

			return allocator;
		}
	}
}
