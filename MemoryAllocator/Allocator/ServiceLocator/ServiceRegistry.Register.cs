using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, MemPtr ptr)
		{
			_typeToPtr.Add(context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(SafePtr<Allocator> allocator, ServiceRegistryContext context, MemPtr ptr)
		{
			_typeToPtr.Add(allocator, context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, Ptr ptr)
		{
			_typeToPtr.Add(context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(SafePtr<Allocator> allocator, ServiceRegistryContext context, Ptr ptr)
		{
			_typeToPtr.Add(allocator, context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(context, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(SafePtr<Allocator> allocator, ServiceRegistryContext context, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(allocator, context, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(MemPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(SafePtr<Allocator> allocator, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(allocator, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(SafePtr<Allocator> allocator, Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(allocator, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Ptr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(SafePtr<Allocator> allocator, Ptr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(allocator, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(indexedPtr.typeIndex, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(SafePtr<Allocator> allocator, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(allocator, indexedPtr.typeIndex, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var serviceContext = ServiceRegistryContext.Create<TBase>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(SafePtr<Allocator> allocator, Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var serviceContext = ServiceRegistryContext.Create<TBase>();
			_typeToPtr.Add(allocator, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}
	}
}
