using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, WPtr ptr)
		{
			_typeToPtr.Add(context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(World world, ServiceRegistryContext context, WPtr ptr)
		{
			_typeToPtr.Add(world, context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, CWPtr ptr)
		{
			_typeToPtr.Add(context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(World world, ServiceRegistryContext context, CWPtr ptr)
		{
			_typeToPtr.Add(world, context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(context, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(World world, ServiceRegistryContext context, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(world, context, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(WPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(World world, WPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(world, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(CWPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(World world, CWPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(world, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(CWPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(World world, CWPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(world, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(indexedPtr.typeIndex, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(World world, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(world, indexedPtr.typeIndex, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(CWPtr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var serviceContext = ServiceRegistryContext.Create<TBase>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(World world, CWPtr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var serviceContext = ServiceRegistryContext.Create<TBase>();
			_typeToPtr.Add(world, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}
	}
}
