using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial class World
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(MemPtr ptr) where T: unmanaged, IIndexedType
		{
			_serviceRegistry.RegisterService<T>(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(CachedPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			_serviceRegistry.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(CachedPtr ptr) where T: unmanaged, IIndexedType
		{
			_serviceRegistry.RegisterService<T>(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			_serviceRegistry.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService<T>() where T: unmanaged, IIndexedType
		{
			_serviceRegistry.RemoveService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(CachedPtr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			_serviceRegistry.RegisterServiceAs<T, TBase>(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(IndexedPtr indexedPtr)
		{
			_serviceRegistry.RemoveService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>() where T: unmanaged, IIndexedType
		{
			return ref _serviceRegistry.GetOrRegisterService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>() where T: unmanaged, IIndexedType
		{
			return _serviceRegistry.GetOrRegisterServicePtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>() where T: unmanaged, IIndexedType
		{
			return ref _serviceRegistry.GetService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(out bool exist) where T: unmanaged, IIndexedType
		{
			return ref _serviceRegistry.GetService<T>(this, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref _serviceRegistry.GetService(this, proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref _serviceRegistry.GetService(this, proxyPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void GetService<T>(out CachedPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = _serviceRegistry.GetServiceCachedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetServiceAs<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref _serviceRegistry.GetServiceAs<TBase, T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged, IIndexedType
		{
			return _serviceRegistry.GetServiceIndexedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>() where T: unmanaged, IIndexedType
		{
			return _serviceRegistry.GetServiceCachedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>() where T: unmanaged, IIndexedType
		{
			return _serviceRegistry.GetServicePtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServiceAsPtr<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return _serviceRegistry.GetServiceAsPtr<TBase, T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasService<T>() where T: unmanaged, IIndexedType
		{
			return _serviceRegistry.HasService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetServicePtr<T>(out SafePtr<T> ptr) where T: unmanaged, IIndexedType
		{
			return _serviceRegistry.TryGetServicePtr<T>(this, out ptr);
		}
	}
}
