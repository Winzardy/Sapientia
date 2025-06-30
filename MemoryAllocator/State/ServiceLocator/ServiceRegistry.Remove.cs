using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(WorldState worldState, ServiceRegistryContext context)
		{
			_typeToPtr.Remove(worldState, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService<T>(WorldState worldState) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Remove(worldState, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(WorldState worldState, IndexedPtr indexedPtr)
		{
			_typeToPtr.Remove(worldState, indexedPtr.typeIndex);
		}
	}
}
