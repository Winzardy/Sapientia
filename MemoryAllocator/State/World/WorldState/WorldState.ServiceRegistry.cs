using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct WorldState
	{
		#region Managed

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ClassPtr<T> RegisterService<T>(T service)
			where T : class, IIndexedType
		{
			var ptr = new ClassPtr<T>(service);
			var typeIndex = TypeIndex.Create<T>();
			ref var servicePtr = ref GetOrRegisterService<ClassPtr<T>>(typeIndex, ServiceType.NoState, out var isExist);
			if (isExist)
				servicePtr.Dispose();

			servicePtr = ptr;
			return ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService<T>()
			where T : class, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			if (!RemoveService<ClassPtr<T>>(typeIndex, ServiceType.NoState, out var servicePtr))
				return;
			servicePtr.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetServiceClass<T>()
			where T : class, IIndexedType
		{
			return GetServiceClassPtr<T>().Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ClassPtr<T> GetServiceClassPtr<T>()
			where T : class, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			var ptr = GetService<ClassPtr<T>>(typeIndex, ServiceType.NoState);
			return ptr;
		}

		#endregion

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetOrCreateService<T>(ServiceType serviceType) where T: unmanaged, IInitializableService
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return ref GetServiceRegistry().GetOrCreateService<T>(this);
				case ServiceType.NoState:
					return ref GetNoStateServiceRegistry().GetOrCreate<T>(this);
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetOrCreateServicePtr<T>(ServiceType serviceType) where T: unmanaged, IInitializableService
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return GetServiceRegistry().GetOrCreateServicePtr<T>(this);
				case ServiceType.NoState:
					return GetNoStateServiceRegistry().GetOrCreatePtr<T>(this);
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}

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
		public readonly bool RemoveService<T>(ServiceType serviceType, out T service) where T: unmanaged, IIndexedType
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return GetServiceRegistry().RemoveService<T>(this, out service);
				case ServiceType.NoState:
					return GetNoStateServiceRegistry().Remove<T>(out service);
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool RemoveService<T>(ServiceRegistryContext context, ServiceType serviceType, out T service) where T: unmanaged
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return GetServiceRegistry().RemoveService<T>(this, context, out service);
				case ServiceType.NoState:
					return GetNoStateServiceRegistry().Remove<T>(context, out service);
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool RemoveService<T>(ServiceType serviceType = ServiceType.WorldState) where T: unmanaged, IIndexedType
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return GetServiceRegistry().RemoveService<T>(this);
				case ServiceType.NoState:
					return GetNoStateServiceRegistry().Remove<T>();
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetOrRegisterService<T>(ServiceType serviceType = ServiceType.WorldState) where T: unmanaged, IIndexedType
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return ref GetServiceRegistry().GetOrRegisterService<T>(this);
				case ServiceType.NoState:
					return ref GetNoStateServiceRegistry().GetOrCreate<T>();
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetOrRegisterService<T>(ServiceRegistryContext context, ServiceType serviceType = ServiceType.WorldState) where T: unmanaged
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return ref GetServiceRegistry().GetOrRegisterService<T>(this, context);
				case ServiceType.NoState:
					return ref GetNoStateServiceRegistry().GetOrCreate<T>(context);
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetOrRegisterService<T>(ServiceRegistryContext context, ServiceType serviceType, out bool isExist) where T: unmanaged
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return ref GetServiceRegistry().GetOrRegisterService<T>(this, context, out isExist);
				case ServiceType.NoState:
					return ref GetNoStateServiceRegistry().GetOrCreate<T>(context, out isExist);
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetOrRegisterServicePtr<T>(ServiceType serviceType = ServiceType.WorldState) where T: unmanaged, IIndexedType
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return GetServiceRegistry().GetOrRegisterServicePtr<T>(this);
				case ServiceType.NoState:
					return GetNoStateServiceRegistry().GetOrCreatePtr<T>();
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetOrRegisterServicePtr<T>(ServiceRegistryContext context, ServiceType serviceType = ServiceType.WorldState)
			where T: unmanaged
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return GetServiceRegistry().GetOrRegisterServicePtr<T>(this, context);
				case ServiceType.NoState:
					return GetNoStateServiceRegistry().GetOrCreatePtr<T>(context);
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetService<T>(ServiceType serviceType = ServiceType.WorldState) where T: unmanaged, IIndexedType
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return ref GetServiceRegistry().GetService<T>(this);
				case ServiceType.NoState:
					return ref GetNoStateServiceRegistry().Get<T>();
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetService<T>(ServiceRegistryContext context, ServiceType serviceType = ServiceType.WorldState) where T: unmanaged
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return ref GetServiceRegistry().GetService<T>(this, context);
				case ServiceType.NoState:
					return ref GetNoStateServiceRegistry().Get<T>(context);
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
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
		public readonly SafePtr<T> GetServicePtr<T>(ServiceType serviceType = ServiceType.WorldState) where T: unmanaged, IIndexedType
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return GetServiceRegistry().GetServicePtr<T>(this);
				case ServiceType.NoState:
					return GetNoStateServiceRegistry().GetPtr<T>();
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
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
		public readonly bool TryGetServicePtr<T>(ServiceType serviceType, out SafePtr<T> ptr) where T: unmanaged, IIndexedType
		{
			switch (serviceType)
			{
				case ServiceType.WorldState:
					return GetServiceRegistry().TryGetServicePtr<T>(this, out ptr);
				case ServiceType.NoState:
					return GetNoStateServiceRegistry().TryGetPtr<T>(out ptr);
				default:
					throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
			}
		}
	}
}
