using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.Extensions
{
	public enum ServiceAccessType
	{
		Default,
		Interlocked,
	}

	public static class ServiceLocator<TService>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TService InterlockedGet()
		{
			return _instance.ReadValue();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void InterlockedSet(TService service)
		{
			_instance.SetValue(service);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TService DefaultGet()
		{
			return _instance.value;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void DefaultSet(TService service)
		{
			_instance.value = service;
		}

		private static event Func<TService> GetInstance = DefaultGet;
		private static event Action<TService> SetInstance = DefaultSet;

		private static AsyncValue<TService> _instance = new (default);

		public static TService Instance
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetInstance();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] private set => SetInstance(value);
		}

		public static ServiceAccessType AccessType
		{
			set
			{
				switch (value)
				{
					case ServiceAccessType.Interlocked:
					{
						GetInstance = InterlockedGet;
						SetInstance = InterlockedSet;
						break;
					}
					default:
					{
						GetInstance = DefaultGet;
						SetInstance = DefaultSet;
						break;
					}
				}
			}
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
		public static TService Get<T>() where T: TService, new()
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
		public static TService Create<T>() where T: TService, new()
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
			Debug.Assert(Instance == null);
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
	}

	public static class ServiceLocator
	{
		#region Get

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Get<TService>() where TService : new()
		{
			return ServiceLocator<TService>.Get<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Get<TService, TConcrete>() where TConcrete : TService, new()
		{
			return ServiceLocator<TService>.Get<TConcrete>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<TService>(out TService service)
		{
			var result = ServiceLocator<TService>.TryGet(out service);

			if (!result)
				throw new Exception($"Not have target service [ {typeof(TService)} ]");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGet<TService>(out TService service)
		{
			return ServiceLocator<TService>.TryGet(out service);
		}


		#endregion

		#region Create

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<TService>() where TService : new()
		{
			return ServiceLocator<TService>.Create<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<TService, TConcrete>() where TConcrete : TService, new()
		{
			return ServiceLocator<TService>.Create<TConcrete>();
		}

		#endregion

		#region Register

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRegisterAsService<TService>(this TService service)
		{
			return ServiceLocator<TService>.TryRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService RegisterAsService<TService>(this TService service)
		{
			return ServiceLocator<TService>.Register(service);
		}

		#endregion

		#region Unregister

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryUnRegisterAsService<TService>(this TService service)
		{
			return ServiceLocator<TService>.TryUnRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegisterAsService<TService>(this TService service)
		{
			ServiceLocator<TService>.UnRegister(service);
		}

		#endregion

		#region Replace

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService<TService>(this TService service)
		{
			return ServiceLocator<TService>.ReplaceService(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService<TService>(this TService service, ServiceAccessType accessType)
		{
			ServiceLocator<TService>.AccessType = accessType;
			return ServiceLocator<TService>.ReplaceService(service);
		}

		#endregion
	}

	public interface IService {}
}
