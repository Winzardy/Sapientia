using System.Runtime.InteropServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
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

			public static implicit operator MemoryBlockRef(AllocatorPtr allocatorPtr)
			{
				return new MemoryBlockRef(allocatorPtr.zoneId, allocatorPtr.zoneOffset - TSize<MemoryBlock>.size);
			}
		}
	}
}
