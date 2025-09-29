using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveService(WorldState worldState, ServiceRegistryContext context)
		{
			return _typeToPtr.Remove(worldState, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveService<T>(WorldState worldState) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return _typeToPtr.Remove(worldState, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveService<T>(WorldState worldState, out T service) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			var result = _typeToPtr.Remove(worldState, typeIndex, out var servicePtr);
			service = servicePtr.GetValue<T>(worldState);

			return result;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveService(WorldState worldState, IndexedPtr indexedPtr)
		{
			return _typeToPtr.Remove(worldState, indexedPtr.typeIndex);
		}
	}
}
