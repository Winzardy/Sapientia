using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct DataAccessor
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(DataAccessorContext context)
		{
			_typeToPtr.Remove(context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(Allocator allocator, DataAccessorContext context)
		{
			_typeToPtr.Remove(allocator, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService<T>() where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Remove(typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService<T>(Allocator allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Remove(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(IndexedPtr indexedPtr)
		{
			_typeToPtr.Remove(indexedPtr.typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(Allocator allocator, IndexedPtr indexedPtr)
		{
			_typeToPtr.Remove(allocator, indexedPtr.typeIndex);
		}
	}
}
