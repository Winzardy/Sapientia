using System.Runtime.InteropServices;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct BlockId
		{
			public int sizeId;
			public int freeId;

			public bool IsFree => freeId >= 0;

			public BlockId(int sizeId, int freeId)
			{
				this.sizeId = sizeId;
				this.freeId = freeId;
			}
		}
	}
}
