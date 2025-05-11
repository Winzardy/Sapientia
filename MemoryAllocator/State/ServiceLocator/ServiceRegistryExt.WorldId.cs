using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public static unsafe partial class ServiceRegistryExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref WorldId worldId, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			worldId.GetWorld().RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref WorldId worldId, CachedPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			worldId.GetWorld().RegisterService(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref WorldId worldId, CachedPtr ptr) where T: unmanaged, IIndexedType
		{
			worldId.GetWorld().RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this ref WorldId worldId, IndexedPtr indexedPtr)
		{
			worldId.GetWorld().RegisterService(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<T>(this ref WorldId worldId) where T: unmanaged, IIndexedType
		{
			worldId.GetWorld().RemoveService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterServiceAs<T, TBase>(this ref WorldId worldId, CachedPtr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			worldId.GetWorld().RegisterServiceAs<T, TBase>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService(this ref WorldId worldId, IndexedPtr indexedPtr)
		{
			worldId.GetWorld().RemoveService(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref WorldId worldId) where T: unmanaged, IIndexedType
		{
			return ref worldId.GetWorld().GetService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref WorldId worldId, out bool exist) where T: unmanaged, IIndexedType
		{
			return ref worldId.GetWorld().TryGetService<T>(out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref WorldId worldId, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref worldId.GetWorld().GetService(proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref WorldId worldId, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref worldId.GetWorld().GetService(proxyPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetService<T>(this ref WorldId worldId, out CachedPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = worldId.GetWorld().GetServiceCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr GetServiceIndexedPtr<T>(this ref WorldId worldId) where T: unmanaged, IIndexedType
		{
			return worldId.GetWorld().GetServiceIndexedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CachedPtr<T> GetServiceCachedPtr<T>(this ref WorldId worldId) where T: unmanaged, IIndexedType
		{
			return worldId.GetWorld().GetServiceCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServicePtr<T>(this ref WorldId worldId) where T: unmanaged, IIndexedType
		{
			return worldId.GetWorld().GetServicePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetServiceAs<TBase, T>(this ref WorldId worldId) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref worldId.GetWorld().GetServiceAs<TBase, T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> GetServiceAsPtr<TBase, T>(this ref WorldId worldId) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return worldId.GetWorld().GetServiceAsPtr<TBase, T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<T>(this ref WorldId worldId) where T: unmanaged, IIndexedType
		{
			return worldId.GetWorld().HasService<T>();
		}
	}
}
