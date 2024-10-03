using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.ServiceManagement
{
	public enum ServiceAccessType
	{
		Default,
		Interlocked,
	}

	public static class SingleService<TService>
	{
		private static AsyncValue<TService> _instance = new (default);
		public static ServiceAccessType AccessType { get; set; }

		public static TService Instance
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (AccessType == ServiceAccessType.Interlocked)
					return _instance.ReadValue();
				return _instance.value;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set
			{

				if (AccessType == ServiceAccessType.Interlocked)
					_instance.SetValue(value);
				else
					_instance.value = value;
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

	public static class SingleService
	{
		#region Get

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Get<TService>() where TService : new()
		{
			return SingleService<TService>.Get<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Get<TService, TConcrete>() where TConcrete : TService, new()
		{
			return SingleService<TService>.Get<TConcrete>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<TService>(out TService service)
		{
			var result = SingleService<TService>.TryGet(out service);

			if (!result)
				throw new Exception($"Not have target service [ {typeof(TService)} ]");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetOrCreate<TService>(out TService service) where TService : new()
		{
			service = SingleService<TService>.Get<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGet<TService>(out TService service)
		{
			return SingleService<TService>.TryGet(out service);
		}


		#endregion

		#region Create

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<TService>() where TService : new()
		{
			return SingleService<TService>.Create<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<TService, TConcrete>() where TConcrete : TService, new()
		{
			return SingleService<TService>.Create<TConcrete>();
		}

		#endregion

		#region Register

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRegisterAsService<TService>(this TService service)
		{
			return SingleService<TService>.TryRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService RegisterAsService<TService>(this TService service)
		{
			return SingleService<TService>.Register(service);
		}

		#endregion

		#region Unregister

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryUnRegisterAsService<TService>(this TService service)
		{
			return SingleService<TService>.TryUnRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegisterAsService<TService>(this TService service)
		{
			SingleService<TService>.UnRegister(service);
		}

		#endregion

		#region Replace

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService<TService>(this TService service)
		{
			return SingleService<TService>.ReplaceService(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService<TService>(this TService service, ServiceAccessType accessType)
		{
			SingleService<TService>.AccessType = accessType;
			return SingleService<TService>.ReplaceService(service);
		}

		#endregion
	}

	public interface IService {}
}
