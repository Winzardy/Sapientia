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

	public struct ContextScope<TContext, TService> : IDisposable where TService : IService
	{
		private TContext _context;

		public ContextScope(TContext context)
		{
			_context = context;
			ServiceLocator<TContext, TService>.SetContext(_context);
		}

		public void Dispose()
		{
			ServiceLocator<TContext, TService>.SetPreviousContext(_context);
		}
	}

	public static class ServiceLocator<TContext, TService> where TService : IService
	{
		private static readonly Dictionary<TContext, TService> SERVICES = new ();

		private static TContext _context;
		private static TContext _previousContext;

		public static TContext Context => _context;

		public static TService Instance => SERVICES[_context];

		public static Dictionary<TContext, TService>.Enumerator GetServicesEnumerator()
		{
			return SERVICES.GetEnumerator();
		}

		public static bool HasContext()
		{
			return Context != null;
		}

		public static bool HasInstance()
		{
			return Instance != null;
		}

		public static void SetContext(TContext newContext)
		{
			SERVICES.TryAdd(newContext, default);
			_previousContext = _context;
			_context = newContext;
		}

		public static void SetPreviousContext(TContext currentContext)
		{
			if (!currentContext.Equals(_context))
				return;
			(_previousContext, _context) = (_context, _previousContext);
		}

		public static void SetPreviousContext()
		{
			(_previousContext, _context) = (_context, _previousContext);
		}

		public static TService RemoveContext(TContext context)
		{
			if (context != null && SERVICES.Remove(context, out var service))
			{
				if (_context != null && _context.Equals(context))
					_context = default;
				return service;
			}

			return default;
		}

		public static void ResetContext()
		{
			_context = default;

			SERVICES.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Get<T>() where T: TService, new()
		{
			Debug.Assert(HasContext());
			if (Instance == null)
				return Create<T>();
			return Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGet(out TService service)
		{
			Debug.Assert(HasContext());
			service = Instance;
			return Instance != null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<T>() where T: TService, new()
		{
			Debug.Assert(HasContext());
			var value = new T();
			Register(value);
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRegister(TService service)
		{
			Debug.Assert(HasContext());
			if (Instance != null)
				return false;
			SERVICES[_context] = service;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Register(TService service)
		{
			Debug.Assert(HasContext());
			SERVICES[_context] = service;

			return Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryUnRegister(TService service)
		{
			Debug.Assert(HasContext());
			Debug.Assert(Instance == null);
			if (Instance == null || !Instance.Equals(service))
				return false;
			SERVICES[_context] = default;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService(TService service)
		{
			Debug.Assert(HasContext());
			var result = Instance;
			SERVICES[_context] = service;
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister()
		{
			Debug.Assert(HasContext());
			SERVICES[_context] = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister(TService service)
		{
			Debug.Assert(HasContext());
			if (Instance == null || !Instance.Equals(service))
				return;
			SERVICES[_context] = default;
		}
	}

	public static class ServiceLocator<TService> where TService: IService
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
		#region Context

		public static ContextScope<TContext, TService> GetContextScope<TContext, TService>(this TContext context) where TService: IService
		{
			return new ContextScope<TContext, TService>(context);
		}

		public static void SetContext<TContext, TService>(this TContext context) where TService: IService
		{
			ServiceLocator<TContext, TService>.SetContext(context);
		}

		public static void RetrievePreviousContext<TContext, TService>(this TContext context) where TService: IService
		{
			ServiceLocator<TContext, TService>.SetPreviousContext(context);
		}

		public static TService RemoveContext<TContext, TService>(this TContext context) where TService: IService
		{
			return ServiceLocator<TContext, TService>.RemoveContext(context);
		}

		public static void ResetContext<TContext, TService>() where TService: IService
		{
			ServiceLocator<TContext, TService>.ResetContext();
		}

		#endregion

		#region Get

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ContextGet<TContext, TService>() where TService: IService, new()
		{
			return ServiceLocator<TContext, TService>.Get<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Get<TService>() where TService: IService, new()
		{
			return ServiceLocator<TService>.Get<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ContextGet<TContext, TService, TConcrete>() where TService: IService where TConcrete : TService, new()
		{
			return ServiceLocator<TContext, TService>.Get<TConcrete>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Get<TService, TConcrete>() where TService: IService where TConcrete : TService, new()
		{
			return ServiceLocator<TService>.Get<TConcrete>();
		}

		#endregion

		#region Create

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ContextCreate<TContext, TService>() where TService: IService, new()
		{
			return ServiceLocator<TContext, TService>.Create<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<TService>() where TService: IService, new()
		{
			return ServiceLocator<TService>.Create<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ContextCreate<TContext, TService, TConcrete>() where TService: IService where TConcrete : TService, new()
		{
			return ServiceLocator<TContext, TService>.Create<TConcrete>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<TService, TConcrete>() where TService: IService where TConcrete : TService, new()
		{
			return ServiceLocator<TService>.Create<TConcrete>();
		}

		#endregion

		#region Register

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ContextTryRegisterAsService<TContext, TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TContext, TService>.TryRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRegisterAsService<TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TService>.TryRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ContextRegisterAsService<TContext, TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TContext, TService>.Register(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService RegisterAsService<TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TService>.Register(service);
		}

		#endregion

		#region Unregister

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ContextTryUnRegisterAsService<TContext, TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TContext, TService>.TryUnRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryUnRegisterAsService<TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TService>.TryUnRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ContextUnRegisterAsService<TContext, TService>(this TService service) where TService: IService
		{
			ServiceLocator<TContext, TService>.UnRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegisterAsService<TService>(this TService service) where TService: IService
		{
			ServiceLocator<TService>.UnRegister(service);
		}

		#endregion

		#region Replace

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ContextReplaceService<TContext, TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TContext, TService>.ReplaceService(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService<TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TService>.ReplaceService(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService<TService>(this TService service, ServiceAccessType accessType) where TService: IService
		{
			ServiceLocator<TService>.AccessType = accessType;
			return ServiceLocator<TService>.ReplaceService(service);
		}

		#endregion
	}

	public interface IService {}
}