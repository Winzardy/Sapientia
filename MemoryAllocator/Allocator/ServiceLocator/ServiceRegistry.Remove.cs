using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(ServiceRegistryContext context)
		{
			_typeToPtr.Remove(context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService<T>() where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Remove(typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(IndexedPtr indexedPtr)
		{
			_typeToPtr.Remove(indexedPtr.typeIndex);
		}
	}
}
