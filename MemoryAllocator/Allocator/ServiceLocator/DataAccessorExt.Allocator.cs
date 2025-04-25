using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class DataAccessorExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this Allocator allocator, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			allocator.dataAccessor.RegisterService<T>(allocator, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this Allocator allocator, Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			allocator.dataAccessor.RegisterService(allocator, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this Allocator allocator, Ptr ptr) where T: unmanaged, IIndexedType
		{
			allocator.dataAccessor.RegisterService<T>(allocator, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this Allocator allocator, IndexedPtr indexedPtr)
		{
			allocator.dataAccessor.RegisterService(allocator, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<T>(this Allocator allocator) where T: unmanaged, IIndexedType
		{
			allocator.dataAccessor.RemoveService<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterServiceAs<T, TBase>(this Allocator allocator, Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			allocator.dataAccessor.RegisterServiceAs<T, TBase>(allocator, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService(this Allocator allocator, IndexedPtr indexedPtr)
		{
			allocator.dataAccessor.RemoveService(allocator, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrRegisterService<T>(this Allocator allocator) where T: unmanaged, IIndexedType
		{
			return ref allocator.dataAccessor.GetOrRegisterService<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetOrRegisterServicePtr<T>(this Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.dataAccessor.GetOrRegisterServicePtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this Allocator allocator) where T: unmanaged, IIndexedType
		{
			return ref allocator.dataAccessor.GetService<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T TryGetService<T>(this Allocator allocator, out bool exist) where T: unmanaged, IIndexedType
		{
			return ref allocator.dataAccessor.GetService<T>(allocator, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this Allocator allocator, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref allocator.dataAccessor.GetService(allocator, proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this Allocator allocator, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref allocator.dataAccessor.GetService(allocator, proxyPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetService<T>(this Allocator allocator, out Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = allocator.dataAccessor.GetServiceCachedPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetServiceAs<TBase, T>(this Allocator allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref allocator.dataAccessor.GetServiceAs<TBase, T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr GetServiceIndexedPtr<T>(this Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.dataAccessor.GetServiceIndexedPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ptr<T> GetServiceCachedPtr<T>(this Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.dataAccessor.GetServiceCachedPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServicePtr<T>(this Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.dataAccessor.GetServicePtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServiceAsPtr<TBase, T>(this Allocator allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return allocator.dataAccessor.GetServiceAsPtr<TBase, T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<T>(this Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.dataAccessor.HasService<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetServicePtr<T>(this Allocator allocator, out SafePtr<T> ptr) where T: unmanaged, IIndexedType
		{
			return allocator.dataAccessor.TryGetServicePtr<T>(allocator, out ptr);
		}
	}
}
