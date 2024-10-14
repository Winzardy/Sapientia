using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe struct ServiceLocator
	{
		private Dictionary<ServiceContext, IndexedPtr> _typeToPtr;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceLocator Create(int capacity = 128)
		{
			return Create(AllocatorManager.CurrentAllocatorPtr, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceLocator Create(Allocator* allocator, int capacity = 128)
		{
			return new ServiceLocator
			{
				_typeToPtr = new Dictionary<ServiceContext, IndexedPtr>(allocator, capacity),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceContext context, MemPtr ptr)
		{
			_typeToPtr.Add(context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceContext context, Ptr ptr)
		{
			_typeToPtr.Add(context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceContext context, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(context, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(MemPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Ptr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceContext.Create<T>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(indexedPtr.typeIndex, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var serviceContext = ServiceContext.Create<TBase>();
			_typeToPtr.Add(serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(ServiceContext context)
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator, ServiceContext context) where T: unmanaged
		{
			return ref _typeToPtr.GetValue(allocator, context).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return ref _typeToPtr.GetValue(allocator, typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator, ServiceContext context, out bool exist) where T: unmanaged
		{
			return ref _typeToPtr.GetValue(allocator, context, out exist).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(out bool exist) where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return ref _typeToPtr.GetValue(allocator, typeIndex, out exist).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			return ref _typeToPtr.GetValue(allocator, proxyPtr.indexedPtr.typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			return ref _typeToPtr.GetValue(allocator, proxyPtr.indexedPtr.typeIndex, out exist).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetServiceAs<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<TBase>();

			return ref _typeToPtr.GetValue(allocator, typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(Allocator* allocator, ServiceContext context) where T: unmanaged
		{
			return _typeToPtr.GetValue(allocator, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>(Allocator* allocator, ServiceContext context) where T: unmanaged
		{
			return _typeToPtr.GetValue(allocator, context).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>(Allocator* allocator, ServiceContext context) where T: unmanaged
		{
			return _typeToPtr.GetValue(allocator, context).GetPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex).GetPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServiceAsPtr<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<TBase>();

			return _typeToPtr.GetValue(allocator, typeIndex).GetPtr<T>();
		}
	}
}
