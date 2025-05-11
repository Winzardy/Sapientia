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
				var zone = _zonesList[i];

				stream.Write(zone.ptr->size);
				E.ASSERT(zone.ptr->size > 0);

				stream.Write(zone.ptr->memory, zone.ptr->size);
			}

			// Записываем свободные блоки
			stream.Write(_freeBlockPools.count);
			for (var i = 0; i < _freeBlockPools.count; i++)
			{
				stream.Write(_freeBlockPools[i].ptr->blockSize);
				stream.Write(_freeBlockPools[i].ptr->Count);

				if (_freeBlockPools[i].ptr->Count == 0)
					continue;

				stream.Write(_freeBlockPools[i].ptr->GetInnerArray(), _freeBlockPools[i].ptr->Count);
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
				var zoneMemoryPtr = allocator._zonesList[i].ptr->memory;

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

				ref var freeBlocks = ref freeBlockPools[i].Value();
				freeBlocks.Count = blocksCount;

				var arrayPtr = freeBlocks.GetInnerArray();
				stream.Read(ref arrayPtr, blocksCount);
			}

			return allocator;
		}
	}
}
