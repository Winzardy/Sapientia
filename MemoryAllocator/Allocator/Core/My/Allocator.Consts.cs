namespace Sapientia.MemoryAllocator.My
{
	public unsafe partial struct Allocator
	{
		public const int MIN_BLOCK_SIZE = 32;
		public const int MIN_ZONE_SIZE = MIN_ZONE_SIZE_IN_KB * 1024;
		public const int MIN_ZONE_SIZE_IN_KB = 512; // 128
	}
}
