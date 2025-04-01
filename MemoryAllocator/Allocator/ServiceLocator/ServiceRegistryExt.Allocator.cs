using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class ServiceRegistryExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this SafePtr<Allocator> allocator, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			allocator.Value().serviceRegistry.RegisterService<T>(allocator, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this SafePtr<Allocator> allocator, Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			allocator.Value().serviceRegistry.RegisterService(allocator, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this SafePtr<Allocator> allocator, Ptr ptr) where T: unmanaged, IIndexedType
		{
			allocator.Value().serviceRegistry.RegisterService<T>(allocator, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this SafePtr<Allocator> allocator, IndexedPtr indexedPtr)
		{
			allocator.Value().serviceRegistry.RegisterService(allocator, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<T>(this SafePtr<Allocator> allocator) where T: unmanaged, IIndexedType
		{
			allocator.Value().serviceRegistry.RemoveService<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterServiceAs<T, TBase>(this SafePtr<Allocator> allocator, Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			allocator.Value().serviceRegistry.RegisterServiceAs<T, TBase>(allocator, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService(this SafePtr<Allocator> allocator, IndexedPtr indexedPtr)
		{
			allocator.Value().serviceRegistry.RemoveService(allocator, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrRegisterService<T>(this SafePtr<Allocator> allocator) where T: unmanaged, IIndexedType
		{
			return ref allocator.Value().serviceRegistry.GetOrRegisterService<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetOrRegisterServicePtr<T>(this SafePtr<Allocator> allocator) where T: unmanaged, IIndexedType
		{
			return allocator.Value().serviceRegistry.GetOrRegisterServicePtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this SafePtr<Allocator> allocator) where T: unmanaged, IIndexedType
		{
			return ref allocator.Value().serviceRegistry.GetService<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T TryGetService<T>(this SafePtr<Allocator> allocator, out bool exist) where T: unmanaged, IIndexedType
		{
			return ref allocator.Value().serviceRegistry.GetService<T>(allocator, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this SafePtr<Allocator> allocator, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref allocator.Value().serviceRegistry.GetService(allocator, proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this SafePtr<Allocator> allocator, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref allocator.Value().serviceRegistry.GetService(allocator, proxyPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetService<T>(this SafePtr<Allocator> allocator, out Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = allocator.Value().serviceRegistry.GetServiceCachedPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetServiceAs<TBase, T>(this SafePtr<Allocator> allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref allocator.Value().serviceRegistry.GetServiceAs<TBase, T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr GetServiceIndexedPtr<T>(this SafePtr<Allocator> allocator) where T: unmanaged, IIndexedType
		{
			return allocator.Value().serviceRegistry.GetServiceIndexedPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ptr<T> GetServiceCachedPtr<T>(this SafePtr<Allocator> allocator) where T: unmanaged, IIndexedType
		{
			return allocator.Value().serviceRegistry.GetServiceCachedPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServicePtr<T>(this SafePtr<Allocator> allocator) where T: unmanaged, IIndexedType
		{
			return allocator.Value().serviceRegistry.GetServicePtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServiceAsPtr<TBase, T>(this SafePtr<Allocator> allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return allocator.Value().serviceRegistry.GetServiceAsPtr<TBase, T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<T>(this SafePtr<Allocator> allocator) where T: unmanaged, IIndexedType
		{
			return allocator.Value().serviceRegistry.HasService<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetServicePtr<T>(this SafePtr<Allocator> allocator, out SafePtr<T> ptr) where T: unmanaged, IIndexedType
		{
			return allocator.Value().serviceRegistry.TryGetServicePtr<T>(allocator, out ptr);
		}
	}
}
