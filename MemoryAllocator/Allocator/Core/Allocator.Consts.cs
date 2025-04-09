namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		public const int MIN_BLOCK_SIZE = 32;
		public const int MIN_BLOCK_ALIGN_MINUS_ONE = MIN_BLOCK_SIZE - 1;
		public const int MIN_BLOCK_SIZE_LOG2 = 5;// MIN_BLOCK_SIZE.Log2();

		public const int MIN_ZONE_SIZE = MIN_ZONE_SIZE_IN_KB * 1024;
		private const int MIN_ZONE_SIZE_IN_KB = MIN_ZONE_SIZE_IN_MB * 1024;
		private const int MIN_ZONE_SIZE_IN_MB = 10;
	}
}
