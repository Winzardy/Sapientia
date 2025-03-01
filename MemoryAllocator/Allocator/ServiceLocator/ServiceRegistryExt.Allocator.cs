using System.Runtime.CompilerServices;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class ServiceRegistryExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref Allocator allocator, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			allocator.serviceRegistry.RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref Allocator allocator, Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			allocator.serviceRegistry.RegisterService(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref Allocator allocator, Ptr ptr) where T: unmanaged, IIndexedType
		{
			allocator.serviceRegistry.RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this ref Allocator allocator, IndexedPtr indexedPtr)
		{
			allocator.serviceRegistry.RegisterService(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			allocator.serviceRegistry.RemoveService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterServiceAs<T, TBase>(this ref Allocator allocator, Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			allocator.serviceRegistry.RegisterServiceAs<T, TBase>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService(this ref Allocator allocator, IndexedPtr indexedPtr)
		{
			allocator.serviceRegistry.RemoveService(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetOrRegisterService<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return ref allocator.serviceRegistry.GetOrRegisterService<T>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetOrRegisterServicePtr<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.serviceRegistry.GetOrRegisterServicePtr<T>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return ref allocator.serviceRegistry.GetService<T>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T TryGetService<T>(this ref Allocator allocator, out bool exist) where T: unmanaged, IIndexedType
		{
			return ref allocator.serviceRegistry.GetService<T>((Allocator*)allocator.AsPointer(), out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref allocator.serviceRegistry.GetService((Allocator*)allocator.AsPointer(), proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref allocator.serviceRegistry.GetService((Allocator*)allocator.AsPointer(), proxyPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetService<T>(this ref Allocator allocator, out Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = allocator.serviceRegistry.GetServiceCachedPtr<T>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetServiceAs<TBase, T>(this ref Allocator allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref allocator.serviceRegistry.GetServiceAs<TBase, T>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr GetServiceIndexedPtr<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.serviceRegistry.GetServiceIndexedPtr<T>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ptr<T> GetServiceCachedPtr<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.serviceRegistry.GetServiceCachedPtr<T>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetServicePtr<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.serviceRegistry.GetServicePtr<T>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetServiceAsPtr<TBase, T>(this ref Allocator allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return allocator.serviceRegistry.GetServiceAsPtr<TBase, T>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.serviceRegistry.HasService<T>((Allocator*)allocator.AsPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* TryGetServicePtr<T>(this ref Allocator allocator, out bool isExist) where T: unmanaged, IIndexedType
		{
			return allocator.serviceRegistry.TryGetServicePtr<T>((Allocator*)allocator.AsPointer(), out isExist);
		}
	}
}
