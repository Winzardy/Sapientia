using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct DataAccessor
	{
		private Dictionary<DataAccessorContext, IndexedPtr> _typeToPtr;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DataAccessor Create(int capacity = 128)
		{
			return Create(AllocatorManager.CurrentAllocator, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DataAccessor Create(Allocator allocator, int capacity = 128)
		{
			return new DataAccessor
			{
				_typeToPtr = new Dictionary<DataAccessorContext, IndexedPtr>(allocator, capacity),
			};
		}
	}
}
