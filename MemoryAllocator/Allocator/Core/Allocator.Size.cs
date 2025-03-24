using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[INLINE(256)]
		public uint GetSize(in MemPtr ptr)
		{
			var block = (MemBlock*)((byte*)zonesList[ptr.zoneId] + ptr.zoneOffset - sizeof(MemBlock));
			return (uint)block->size;
		}

		[INLINE(256)]
		public readonly void GetSize(out int reservedSize, out int usedSize, out int freeSize)
		{
			usedSize = 0;
			reservedSize = 0;
			for (var i = 0; i < zonesListCount; i++)
			{
				var zone = zonesList[i];
				if (zone != null)
				{
					reservedSize += zone->size;
					usedSize += zone->size;
					usedSize -= GetMzFreeMemory(zone);
				}
			}

			freeSize = reservedSize - usedSize;
		}

		[INLINE(256)]
		public readonly int GetReservedSize()
		{
			var size = 0;
			for (var i = 0; i < zonesListCount; i++)
			{
				var zone = zonesList[i];
				if (zone != null)
				{
					size += zone->size;
				}
			}

			return size;
		}

		[INLINE(256)]
		public readonly int GetUsedSize()
		{
			var size = 0;
			for (var i = 0; i < zonesListCount; i++)
			{
				var zone = zonesList[i];
				if (zone != null)
				{
					size += zone->size;
					size -= GetMzFreeMemory(zone);
				}
			}

			return size;
		}

		[INLINE(256)]
		public readonly int GetFreeSize()
		{
			var size = 0;
			for (var i = 0; i < zonesListCount; i++)
			{
				var zone = zonesList[i];
				if (zone != null)
				{
					size += GetMzFreeMemory(zone);
				}
			}

			return size;
		}
	}
}
