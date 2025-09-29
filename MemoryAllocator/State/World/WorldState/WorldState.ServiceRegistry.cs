using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct WorldState
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void RegisterService<T>(MemPtr ptr) where T: unmanaged, IIndexedType
		{
			GetServiceRegistry().RegisterService<T>(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void RegisterService<T>(CachedPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			GetServiceRegistry().RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void RegisterService<T>(CachedPtr ptr) where T: unmanaged, IIndexedType
		{
			GetServiceRegistry().RegisterService<T>(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void RegisterService(IndexedPtr indexedPtr)
		{
			GetServiceRegistry().RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void RemoveService<T>() where T: unmanaged, IIndexedType
		{
			GetServiceRegistry().RemoveService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void RegisterServiceAs<T, TBase>(CachedPtr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			GetServiceRegistry().RegisterServiceAs<T, TBase>(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void RemoveService(IndexedPtr indexedPtr)
		{
			GetServiceRegistry().RemoveService(this, indexedPtr);
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetOrCreateService<T>() where T: unmanaged, IInitializableService
		{
			return ref GetServiceRegistry().GetOrCreateService<T>(this);
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetOrCreateServicePtr<T>() where T: unmanaged, IInitializableService
		{
			return GetServiceRegistry().GetOrCreateServicePtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetOrRegisterService<T>() where T: unmanaged, IIndexedType
		{
			return ref GetServiceRegistry().GetOrRegisterService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetOrRegisterServicePtr<T>() where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().GetOrRegisterServicePtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetService<T>() where T: unmanaged, IIndexedType
		{
			return ref GetServiceRegistry().GetService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T TryGetService<T>(out bool exist) where T: unmanaged, IIndexedType
		{
			return ref GetServiceRegistry().GetService<T>(this, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetService<T>(ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref GetServiceRegistry().GetService(this, proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetService<T>(ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref GetServiceRegistry().GetService(this, proxyPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void GetService<T>(out CachedPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = GetServiceRegistry().GetServiceCachedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetServiceAs<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref GetServiceRegistry().GetServiceAs<TBase, T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().GetServiceIndexedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly CachedPtr<T> GetServiceCachedPtr<T>() where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().GetServiceCachedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetServicePtr<T>() where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().GetServicePtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetServiceAsPtr<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return GetServiceRegistry().GetServiceAsPtr<TBase, T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool HasService<T>() where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().HasService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool TryGetServicePtr<T>(out SafePtr<T> ptr) where T: unmanaged, IIndexedType
		{
			return GetServiceRegistry().TryGetServicePtr<T>(this, out ptr);
		}
	}
}
