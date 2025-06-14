using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct World
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(MemPtr ptr) where T: unmanaged, IIndexedType
		{
			GetServiceRegistry().RegisterService<T>(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(CachedPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			GetServiceRegistry().RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(CachedPtr ptr) where T: unmanaged, IIndexedType
		{
			GetServiceRegistry().RegisterService<T>(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			GetServiceRegistry().RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService<T>() where T: unmanaged, IIndexedType
		{
			GetServiceRegistry().RemoveService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(CachedPtr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			GetServiceRegistry().RegisterServiceAs<T, TBase>(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(IndexedPtr indexedPtr)
		{
			GetServiceRegistry().RemoveService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>() where T: unmanaged, IIndexedType
		{
			return ref GetServiceRegistry().GetOrRegisterService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>() where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().GetOrRegisterServicePtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>() where T: unmanaged, IIndexedType
		{
			return ref GetServiceRegistry().GetService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(out bool exist) where T: unmanaged, IIndexedType
		{
			return ref GetServiceRegistry().GetService<T>(this, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref GetServiceRegistry().GetService(this, proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref GetServiceRegistry().GetService(this, proxyPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void GetService<T>(out CachedPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = GetServiceRegistry().GetServiceCachedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetServiceAs<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref GetServiceRegistry().GetServiceAs<TBase, T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().GetServiceIndexedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>() where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().GetServiceCachedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>() where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().GetServicePtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServiceAsPtr<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return GetServiceRegistry().GetServiceAsPtr<TBase, T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasService<T>() where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().HasService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetServicePtr<T>(out SafePtr<T> ptr) where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().TryGetServicePtr<T>(this, out ptr);
		}
	}
}
