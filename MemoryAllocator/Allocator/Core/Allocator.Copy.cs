using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		public void CopyFrom(Allocator* other)
		{
			if (other->zonesList == null)
			{
				if (zonesList != null)
					FreeZones();
				return;
			}

			if (zonesListCount < other->zonesListCount)
			{
				for (var i = zonesListCount; i < other->zonesListCount; ++i)
				{
					var otherZone = other->zonesList[i];
					if (otherZone == null)
					{
						AddZone(null, false);
					}
					else
					{
						var zone = MzCreateZone(otherZone->size);
						AddZone(zone, false);
					}
				}
			}

			if (zonesListCount == other->zonesListCount)
			{
				for (var i = 0; i < other->zonesListCount; ++i)
				{
					ref var currentZone = ref zonesList![i];
					var otherZone = other->zonesList[i];
					{
						if (currentZone == null && otherZone == null)
							continue;

						if (currentZone == null)
						{
							currentZone = MzCreateZone(otherZone->size);
							MemoryExt.MemCopy(otherZone, currentZone, otherZone->size);
						}
						else if (otherZone == null)
						{
							MzFreeZone(currentZone);
							currentZone = null;
						}
						else
						{
							// resize zone
							currentZone = MzReallocZone(currentZone, otherZone->size);
							MemoryExt.MemCopy(otherZone, currentZone, otherZone->size);
						}
					}
				}
			}
			else
			{
				FreeZones();

				for (var i = 0; i < other->zonesListCount; i++)
				{
					var otherZone = other->zonesList[i];

					if (otherZone != null)
					{
						var zone = MzCreateZone(otherZone->size);
						MemoryExt.MemCopy(otherZone, zone, otherZone->size);
						AddZone(zone, false);
					}
					else
					{
						AddZone(null, false);
					}
				}
			}

			threadId = other->threadId;
			initialSize = other->initialSize;
			serviceRegistry = other->serviceRegistry;

			version = (ushort)(other->version + 1);
		}

		public MemPtr CopyPtrTo(Allocator* dstAllocator, in MemPtr ptr)
		{
			if (!ptr.IsValid())
				return MemPtr.Invalid;
			if (ptr.IsZeroSized())
				return ptr;

			var size = GetSize(ptr);
			var dstPtr = dstAllocator->MemAlloc(size);

			var srcData = GetUnsafePtr(ptr);
			var dstData = dstAllocator->GetUnsafePtr(dstPtr);

			MemoryExt.MemCopy(srcData, dstData, size);

			return dstPtr;
		}
	}
}
