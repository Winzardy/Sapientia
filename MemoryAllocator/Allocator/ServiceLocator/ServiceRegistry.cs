using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct ServiceRegistry
	{
		private Dictionary<ServiceRegistryContext, IndexedPtr> _typeToPtr;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceRegistry Create(int capacity = 128)
		{
			return Create(AllocatorManager.CurrentAllocatorPtr, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceRegistry Create(SafePtr<Allocator> allocator, int capacity = 128)
		{
			return new ServiceRegistry
			{
				_typeToPtr = new Dictionary<ServiceRegistryContext, IndexedPtr>(allocator, capacity),
			};
		}
	}
}
