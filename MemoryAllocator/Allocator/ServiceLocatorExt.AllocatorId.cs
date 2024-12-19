using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class ServiceLocatorExt
	{
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetLocalService<T>(this ref AllocatorId allocatorId)
		{
			return ServiceManagement.ServiceLocator<T>.GetService(allocatorId);
		}
	}
}
