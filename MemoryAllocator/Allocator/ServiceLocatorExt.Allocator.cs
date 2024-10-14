using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class ServiceLocatorExt
	{
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
	}
}
