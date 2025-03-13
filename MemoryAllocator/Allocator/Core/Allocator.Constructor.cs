using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[INLINE(256)]
		public Allocator Initialize(AllocatorId allocatorId, int initialSize)
		{
			initialSize = initialSize.Max(MIN_ZONE_SIZE);

			this.initialSize = initialSize;
			this.allocatorId = allocatorId;

			var zone = MzCreateZone(initialSize);
			AddZone(zone);

			threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
			version = 1;

			serviceRegistry = ServiceRegistry.Create((Allocator*)this.AsPointer());

			return this;
		}

		[INLINE(256)]
		public void Dispose()
		{
			FreeZones();

			if (zonesList != null)
			{
				MemoryExt.MemFree(zonesList);
				zonesList = null;
			}

			this = default;
		}
	}
}
