using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		private Dictionary<ServiceRegistryContext, IndexedPtr> _typeToPtr;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceRegistry Create(int capacity = 128)
		{
			return Create(WorldManager.CurrentWorld, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceRegistry Create(World world, int capacity = 128)
		{
			return new ServiceRegistry
			{
				_typeToPtr = new Dictionary<ServiceRegistryContext, IndexedPtr>(world, capacity),
			};
		}
	}
}
