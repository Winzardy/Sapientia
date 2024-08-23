namespace Sapientia.MemoryAllocator
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
				var list = new System.Collections.Generic.List<Dump>();
				for (var i = 0; i < _allocator.zonesListCount; ++i)
				{
					var zone = _allocator.zonesList[i];

					if (zone == null)
					{
						list.Add(default);
						continue;
					}

					var blocks = new System.Collections.Generic.List<string>();
					Allocator.MzDumpHeap(zone, blocks);
					var item = new Dump() { blocks = blocks.ToArray(), };
					list.Add(item);
				}

				return list.ToArray();
			}
		}

		public Dump[] Checks
		{
			get
			{
				var list = new System.Collections.Generic.List<Dump>();
				for (var i = 0; i < _allocator.zonesListCount; ++i)
				{
					var zone = _allocator.zonesList[i];

					if (zone == null)
					{
						list.Add(default);
						continue;
					}

					var blocks = new System.Collections.Generic.List<string>();
					Allocator.MzCheckHeap(zone, blocks);
					var item = new Dump() { blocks = blocks.ToArray(), };
					list.Add(item);
				}

				return list.ToArray();
			}
		}
	}
}
