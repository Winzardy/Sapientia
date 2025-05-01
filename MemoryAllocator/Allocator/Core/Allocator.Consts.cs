namespace Sapientia.MemoryAllocator
{
	public unsafe partial class Allocator
	{
		public const int BLOCK_ALIGN = 8;
		public const int BLOCK_ALIGN_MINUS_ONE = BLOCK_ALIGN - 1;

		public const int MIN_BLOCK_SIZE = 32;
		public const int MIN_BLOCK_SIZE_LOG2 = 5;// MIN_BLOCK_SIZE.Log2();

		public const int MIN_ZONE_SIZE = MIN_ZONE_SIZE_IN_KB * 1024;
		private const int MIN_ZONE_SIZE_IN_KB = MIN_ZONE_SIZE_IN_MB * 1024;
		private const int MIN_ZONE_SIZE_IN_MB = 30;
	}
}
