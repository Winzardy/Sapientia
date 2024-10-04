using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe struct ServiceLocator
	{
		private Dictionary<TypeIndex, IndexedPtr> _typeToPtr;

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
				_typeToPtr = new Dictionary<TypeIndex, IndexedPtr>(allocator, capacity),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(MemPtr ptr) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Add(typeIndex, new IndexedPtr(ptr, typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Add(typeIndex, new IndexedPtr(ptr, typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Ptr ptr) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Add(typeIndex, new IndexedPtr(ptr, typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(indexedPtr.typeIndex, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var typeIndex = TypeIndex.Create<TBase>();
			_typeToPtr.Add(typeIndex, new IndexedPtr(ptr, typeIndex));
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
		public ref T GetService<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return ref _typeToPtr.GetValue(allocator, typeIndex).GetValue<T>(allocator);
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
		public IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex).GetCachedPtr<T>();
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

	public static unsafe class ServiceLocatorExt
	{
		#region Allocator

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref Allocator allocator, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			allocator.serviceLocator.RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref Allocator allocator, Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			allocator.serviceLocator.RegisterService(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref Allocator allocator, Ptr ptr) where T: unmanaged, IIndexedType
		{
			allocator.serviceLocator.RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this ref Allocator allocator, IndexedPtr indexedPtr)
		{
			allocator.serviceLocator.RegisterService(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			allocator.serviceLocator.RemoveService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterServiceAs<T, TBase>(this ref Allocator allocator, Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			allocator.serviceLocator.RegisterServiceAs<T, TBase>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService(this ref Allocator allocator, IndexedPtr indexedPtr)
		{
			allocator.serviceLocator.RemoveService(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return ref allocator.serviceLocator.GetService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator, out bool exist) where T: unmanaged, IIndexedType
		{
			return ref allocator.serviceLocator.GetService<T>(out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref allocator.serviceLocator.GetService(proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref allocator.serviceLocator.GetService(proxyPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetService<T>(this ref Allocator allocator, out Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = allocator.serviceLocator.GetServiceCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetServiceAs<TBase, T>(this ref Allocator allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref allocator.serviceLocator.GetServiceAs<TBase, T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr GetServiceIndexedPtr<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.serviceLocator.GetServiceIndexedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ptr<T> GetServiceCachedPtr<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.serviceLocator.GetServiceCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetServicePtr<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.serviceLocator.GetServicePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetServiceAsPtr<TBase, T>(this ref Allocator allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return allocator.serviceLocator.GetServiceAsPtr<TBase, T>();
		}

		#endregion

		#region AllocatorId

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref AllocatorId allocatorId, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocator().serviceLocator.RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref AllocatorId allocatorId, Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocator().serviceLocator.RegisterService(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref AllocatorId allocatorId, Ptr ptr) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocator().serviceLocator.RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this ref AllocatorId allocatorId, IndexedPtr indexedPtr)
		{
			allocatorId.GetAllocator().serviceLocator.RegisterService(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocator().serviceLocator.RemoveService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterServiceAs<T, TBase>(this ref AllocatorId allocatorId, Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			allocatorId.GetAllocator().serviceLocator.RegisterServiceAs<T, TBase>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService(this ref AllocatorId allocatorId, IndexedPtr indexedPtr)
		{
			allocatorId.GetAllocator().serviceLocator.RemoveService(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return ref allocatorId.GetAllocator().serviceLocator.GetService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId, out bool exist) where T: unmanaged, IIndexedType
		{
			return ref allocatorId.GetAllocator().serviceLocator.GetService<T>(out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref allocatorId.GetAllocator().serviceLocator.GetService(proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref allocatorId.GetAllocator().serviceLocator.GetService(proxyPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetService<T>(this ref AllocatorId allocatorId, out Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = allocatorId.GetAllocator().serviceLocator.GetServiceCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr GetServiceIndexedPtr<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return allocatorId.GetAllocator().serviceLocator.GetServiceIndexedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ptr<T> GetServiceCachedPtr<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return allocatorId.GetAllocator().serviceLocator.GetServiceCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetServicePtr<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return allocatorId.GetAllocator().serviceLocator.GetServicePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetServiceAs<TBase, T>(this ref AllocatorId allocatorId) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref allocatorId.GetAllocator().serviceLocator.GetServiceAs<TBase, T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetServiceAsPtr<TBase, T>(this ref AllocatorId allocatorId) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return allocatorId.GetAllocator().serviceLocator.GetServiceAsPtr<TBase, T>();
		}

		#endregion
	}
}
