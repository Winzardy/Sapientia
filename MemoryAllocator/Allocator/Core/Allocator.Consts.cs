//#define MEMORY_ALLOCATOR_BOUNDS_CHECK
//#define BURST

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		public const int MIN_ZONE_SIZE = MIN_ZONE_SIZE_IN_KB * 1024;
		public const int MIN_ZONE_SIZE_IN_KB = 512; // 128

		public const byte BLOCK_STATE_FREE = 0;
		public const byte BLOCK_STATE_USED = 1;

		private const int MIN_ZONES_LIST_CAPACITY = 20;
		private const int MIN_FRAGMENT = 64;

		private const int ZONE_ID = 0x1d4a11; // For Debug
	}
}
