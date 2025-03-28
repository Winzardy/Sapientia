using System.Runtime.CompilerServices;
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

			public bool IsFree
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => freeId >= 0;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public BlockId(int sizeId, int freeId)
			{
				this.sizeId = sizeId;
				this.freeId = freeId;
			}
		}
	}
}
