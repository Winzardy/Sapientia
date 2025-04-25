using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct DataAccessor
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(DataAccessorContext context, MemPtr ptr)
		{
			_typeToPtr.Add(context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator allocator, DataAccessorContext context, MemPtr ptr)
		{
			_typeToPtr.Add(allocator, context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(DataAccessorContext context, Ptr ptr)
		{
			_typeToPtr.Add(context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator allocator, DataAccessorContext context, Ptr ptr)
		{
			_typeToPtr.Add(allocator, context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(DataAccessorContext context, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(context, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator allocator, DataAccessorContext context, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(allocator, context, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(MemPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = DataAccessorContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Allocator allocator, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = DataAccessorContext.Create<T>();
			_typeToPtr.Add(allocator, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = DataAccessorContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Allocator allocator, Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = DataAccessorContext.Create<T>();
			_typeToPtr.Add(allocator, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Ptr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = DataAccessorContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Allocator allocator, Ptr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = DataAccessorContext.Create<T>();
			_typeToPtr.Add(allocator, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(indexedPtr.typeIndex, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator allocator, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(allocator, indexedPtr.typeIndex, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var serviceContext = DataAccessorContext.Create<TBase>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(Allocator allocator, Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var serviceContext = DataAccessorContext.Create<TBase>();
			_typeToPtr.Add(allocator, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}
	}
}
