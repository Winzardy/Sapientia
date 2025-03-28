using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class ServiceRegistryExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref AllocatorId allocatorId, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocatorPtr().RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref AllocatorId allocatorId, Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocatorPtr().RegisterService(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref AllocatorId allocatorId, Ptr ptr) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocatorPtr().RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this ref AllocatorId allocatorId, IndexedPtr indexedPtr)
		{
			allocatorId.GetAllocatorPtr().RegisterService(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocatorPtr().RemoveService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterServiceAs<T, TBase>(this ref AllocatorId allocatorId, Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			allocatorId.GetAllocatorPtr().RegisterServiceAs<T, TBase>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService(this ref AllocatorId allocatorId, IndexedPtr indexedPtr)
		{
			allocatorId.GetAllocatorPtr().RemoveService(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return ref allocatorId.GetAllocatorPtr().GetService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId, out bool exist) where T: unmanaged, IIndexedType
		{
			return ref allocatorId.GetAllocatorPtr().TryGetService<T>(out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref allocatorId.GetAllocatorPtr().GetService(proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref allocatorId.GetAllocatorPtr().GetService(proxyPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetService<T>(this ref AllocatorId allocatorId, out Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = allocatorId.GetAllocatorPtr().GetServiceCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr GetServiceIndexedPtr<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return allocatorId.GetAllocatorPtr().GetServiceIndexedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ptr<T> GetServiceCachedPtr<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return allocatorId.GetAllocatorPtr().GetServiceCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServicePtr<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return allocatorId.GetAllocatorPtr().GetServicePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetServiceAs<TBase, T>(this ref AllocatorId allocatorId) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref allocatorId.GetAllocatorPtr().GetServiceAs<TBase, T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServiceAsPtr<TBase, T>(this ref AllocatorId allocatorId) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return allocatorId.GetAllocatorPtr().GetServiceAsPtr<TBase, T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return allocatorId.GetAllocatorPtr().HasService<T>();
		}
	}
}
