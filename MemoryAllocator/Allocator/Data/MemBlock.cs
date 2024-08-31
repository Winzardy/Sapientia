using System.Runtime.InteropServices;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MemBlock
	{
		public int size; // including the header and possibly tiny fragments

		public byte state;

		// to align block
		public byte b1;
		public byte b2;
		public byte b3;
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
		public int id; // should be ZONE_ID
#endif
		public MemBlockOffset next;
		public MemBlockOffset prev;
	}
}
