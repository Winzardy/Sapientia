using Game.App.ServiceManagement;
using System;
using System.Runtime.CompilerServices;

namespace Sapientia.ServiceManagement
{
	public static class ServiceLocator
	{
		private static IServicesSupplier _supplier;

		public static void SetServiceSupplier(IServicesSupplier supplier)
		{
			_supplier = supplier;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService GetOrCreate<TService>() where TService : new()
		{
			if (_supplier != null &&
				_supplier.TryGet<TService>(out var service))
			{
				if (ServiceLocator<TService>.HasInstance())
				{
#if CLIENT
					UnityEngine.Debug.LogError($"Duplicate instance of [ {typeof(TService).Name} ] detected");
#endif
				}

				return service;
			}

			return ServiceLocator<TService>.GetOrCreate<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetOrCreate<TService>(out TService service) where TService : new()
		{
			service = GetOrCreate<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Get<TService>()
		{
			if (_supplier == null)
				throw new Exception("Service Supplier is null.");

			return _supplier.Get<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<TService>(out TService service)
		{
			service = Get<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGet<TService>(out TService service)
		{
			if (_supplier == null)
				throw new Exception("Service Supplier is null.");

			return _supplier.TryGet(out service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService RegisterAsService<TService>(this TService service)
		{
			return ServiceLocator<TService>.Register(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegisterAsService<TService>(this TService service)
		{
			ServiceLocator<TService>.UnRegister(service);
		}
	}
}
