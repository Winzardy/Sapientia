using Game.App.ServiceManagement;
using Sapientia.Collections;
using Sapientia.Data;
using System;
using System.Runtime.CompilerServices;

namespace Sapientia.ServiceManagement
{
	public static partial class ServiceLocator<TService>
	{
		internal static readonly SimpleList<IContextSubscriber> contextSubscribers = new();

		private static AsyncValue<TService> _instance = new(default);

		public static TService Instance
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance.ReadValue();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set => _instance.SetValue(value);
		}

		public static bool HasInstance()
		{
			return Instance != null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsRegistered(TService service)
		{
			return Instance != null && Instance.Equals(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService GetOrCreate<T>() where T : TService, new()
		{
			if (Instance == null)
				return Create<T>();
			return Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGet(out TService service)
		{
			service = Instance;
			return Instance != null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<T>() where T : TService, new()
		{
			var value = new T();
			Register(value);
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRegister(TService service)
		{
			if (Instance != null)
				return false;
			Instance = service;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Register(TService service)
		{
			E.ASSERT(Instance == null);
			Instance = service;

			return Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryUnRegister(TService service)
		{
			if (Instance == null || !Instance.Equals(service))
				return false;
			Instance = default;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService(TService service)
		{
			var result = Instance;
			Instance = service;
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister()
		{
			Instance = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister(TService service)
		{
			if (Instance == null || !Instance.Equals(service))
				return;
			Instance = default;
		}

		public static void RemoveAllContext(bool dispose = false)
		{
			foreach (var subscriber in contextSubscribers)
			{
				subscriber.RemoveAllContext(dispose);
			}
		}
	}

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
