#define MEMORY_ALLOCATOR_BOUNDS_CHECK
//#define BURST

using System;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator : IDisposable
	{
		public static Allocator* Deserialize(ref StreamBufferReader stream)
		{
			var allocator = MemoryExt.MemAlloc<Allocator>();

			stream.Read(ref allocator->version);
			stream.Read(ref allocator->initialSize);
			stream.Read(ref allocator->zonesListCount);
			stream.Read(ref allocator->zonesListCapacity);
			stream.Read(ref allocator->allocatorId);
			stream.Read(ref allocator->serviceRegistry);

			allocator->zonesListCapacity = allocator->zonesListCount;
			allocator->zonesList = (MemZone**)MemoryExt.MemAlloc(allocator->zonesListCapacity * (uint)sizeof(MemZone*), TAlign<IntPtr>.align);
			MemoryExt.MemClear(allocator->zonesList, sizeof(MemZone*) * allocator->zonesListCapacity);

			for (var i = 0; i < allocator->zonesListCount; ++i)
			{
				var length = 0;
				stream.Read(ref length);
				if (length == 0)
					continue;

				var zone = MzCreateZone(length);
				allocator->zonesList[i] = zone;
				var readSize = length;
				var zoneRaw = (byte*)zone;

				stream.Read(ref zoneRaw, readSize);
			}

			return allocator;
		}

		public readonly void Serialize(ref StreamBufferWriter stream)
		{
			stream.Write(version);
			stream.Write(initialSize);
			stream.Write(zonesListCount);
			stream.Write(zonesListCapacity);
			stream.Write(allocatorId);
			stream.Write(serviceRegistry);

			for (var i = 0; i < zonesListCount; ++i)
			{
				var zone = zonesList[i];
				if (zone == null)
				{
					stream.Write(0);
					continue;
				}

				stream.Write(zone->size);
				if (zone->size == 0)
					continue;

				var writeSize = zone->size;
				stream.Write((byte*)zone, writeSize);
			}
		}
	}
}
