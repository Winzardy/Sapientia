using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	public static class ServiceLocator<TContext, TService> where TService : IService
	{
		private static readonly Dictionary<TContext, TService> SERVICES = new ();

		// ReSharper disable once StaticMemberInGenericType
		private static TContext _context;

		public static TContext Context => _context;

		public static TService Instance => SERVICES[_context];

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
			_context = newContext;
		}

		public static void RemoveContext(TContext context)
		{
			if (context != null && SERVICES.Remove(context))
			{
				if (!_context.Equals(context))
					return;
				_context = default;
			}
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
		public static TService Instance { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

		public static bool HasInstance()
		{
			return Instance != null;
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

		public static void SetContext<TContext, TService>(this ref TContext context) where TContext : struct, IServiceContext where TService: IService
		{
			ServiceLocator<TContext, TService>.SetContext(ref context);
		}

		public static void SetContext<TContext, TService>(this TContext context) where TContext : class, IServiceContext where TService: IService
		{
			ServiceLocator<TContext, TService>.SetContext(ref context);
		}

		public static void RemoveContext<TContext, TService>(this TContext context) where TContext : IServiceContext where TService: IService
		{
			ServiceLocator<TContext, TService>.RemoveContext(context);
		}

		public static void ResetContext<TContext, TService>() where TContext : IServiceContext where TService: IService
		{
			ServiceLocator<TContext, TService>.ResetContext();
		}

		#endregion

		#region Get

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ContextGet<TContext, TService>() where TContext : IServiceContext where TService: IService, new()
		{
			return ServiceLocator<TContext, TService>.Get<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Get<TService>() where TService: IService, new()
		{
			return ServiceLocator<TService>.Get<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ContextGet<TContext, TService, TConcrete>() where TContext : IServiceContext where TService: IService where TConcrete : TService, new()
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
		public static TService ContextCreate<TContext, TService>() where TContext : IServiceContext where TService: IService, new()
		{
			return ServiceLocator<TContext, TService>.Create<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<TService>() where TService: IService, new()
		{
			return ServiceLocator<TService>.Create<TService>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ContextCreate<TContext, TService, TConcrete>() where TContext : IServiceContext where TService: IService where TConcrete : TService, new()
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
		public static bool ContextTryRegisterAsService<TContext, TService>(this TService service) where TContext : IServiceContext where TService: IService
		{
			return ServiceLocator<TContext, TService>.TryRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRegisterAsService<TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TService>.TryRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ContextRegisterAsService<TContext, TService>(this TService service) where TContext : IServiceContext where TService: IService
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
		public static bool ContextTryUnRegisterAsService<TContext, TService>(this TService service) where TContext : IServiceContext where TService: IService
		{
			return ServiceLocator<TContext, TService>.TryUnRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryUnRegisterAsService<TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TService>.TryUnRegister(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ContextUnRegisterAsService<TContext, TService>(this TService service) where TContext : IServiceContext where TService: IService
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
		public static TService ContextReplaceService<TContext, TService>(this TService service) where TContext : IServiceContext where TService: IService
		{
			return ServiceLocator<TContext, TService>.ReplaceService(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService<TService>(this TService service) where TService: IService
		{
			return ServiceLocator<TService>.ReplaceService(service);
		}

		#endregion
	}

	public interface IService {}

	public interface IServiceContext
	{
		internal int ContextId { get; set; }
	}
}