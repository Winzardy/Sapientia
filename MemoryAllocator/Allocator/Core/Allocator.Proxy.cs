using Sapientia.Collections;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		public unsafe class AllocatorProxy
		{
			public struct Dump
			{
				public string[] blocks;
			}

			private readonly Allocator _allocator;

			public AllocatorProxy(Allocator allocator)
			{
				this._allocator = allocator;
			}

			public Dump[] Dumps
			{
				get
				{
					using var list = new SimpleList<Dump>();
					for (var i = 0; i < _allocator._zonesList.count; ++i)
					{
						var zone = _allocator._zonesList.ptr.Slice(i);
						using var blocks = new SimpleList<string>();

						MemoryZoneDumpHeap(zone, blocks);

						var item = new Dump() { blocks = blocks.ToArray(), };
						list.Add(item);
					}

					return list.ToArray();
				}
			}
		}
	}
}
