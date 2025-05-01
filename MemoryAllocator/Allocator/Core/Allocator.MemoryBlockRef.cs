using System.Runtime.InteropServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial class Allocator
	{
		[StructLayout(LayoutKind.Sequential)]
		public readonly struct MemoryBlockRef
		{
			public readonly int memoryZoneId;
			public readonly int memoryZoneOffset;

			public MemoryBlockRef(int memoryZoneId, int memoryZoneOffset)
			{
				this.memoryZoneId = memoryZoneId;
				this.memoryZoneOffset = memoryZoneOffset;
			}

			public static implicit operator MemoryBlockRef(MemPtr memPtr)
			{
				return new MemoryBlockRef(memPtr.zoneId, memPtr.zoneOffset - TSize<MemoryBlock>.size);
			}
		}
	}
}
