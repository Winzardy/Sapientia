using System.Runtime.InteropServices;

namespace Sapientia.MemoryAllocator.My
{
	public unsafe partial struct Allocator
	{
		[StructLayout(LayoutKind.Sequential)]
		private readonly struct MemoryBlockRef
		{
			public readonly int memoryZoneId;
			public readonly int memoryZoneOffset;

			public MemoryBlockRef(int memoryZoneId, int memoryZoneOffset)
			{
				this.memoryZoneId = memoryZoneId;
				this.memoryZoneOffset = memoryZoneOffset;
			}
		}
	}
}
