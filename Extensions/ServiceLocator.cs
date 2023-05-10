using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Collections;

namespace Sapientia.Extensions
{
	public static class ServiceLocator<TContext, TService> where TContext : IServiceContext where TService : IService
	{
		private static readonly OrderedSparseSet<(TContext context, TService service)> SERVICES = new ();

		private static int _currentContextId = -1;

		public static ref TContext Context => ref SERVICES.GetValue(_currentContextId).context;

		public static ref TService Instance => ref SERVICES.GetValue(_currentContextId).service;

		public static bool HasContext()
		{
			return _currentContextId >= 0 && Context != null;
		}

		public static bool HasInstance()
		{
			return Instance != null;
		}

		public static void SetContext<T>(ref T newContext) where T: TContext
		{
			if (!SERVICES.HasIndexId(newContext.ContextId) || !SERVICES.GetValue(newContext.ContextId).context.Equals(newContext))
			{
				newContext.ContextId = SERVICES.AllocateIndexId();
				SERVICES.GetValue(newContext.ContextId).context = newContext;
			}

			_currentContextId = newContext.ContextId;
		}

		public static void RemoveContext<T>(T context) where T: TContext
		{
			if (Context != null && Context.ContextId == context.ContextId)
			{
				if (!Context.Equals(context))
					return;
				_currentContextId = -1;
			}

			SERVICES.ReleaseIndexId(context.ContextId, true);
		}

		public static void ResetContext()
		{
			Context = default;
			Instance = default;

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
			Instance = service;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Register(TService service)
		{
			Debug.Assert(HasContext());
			Instance = service;

			return Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryUnRegister(TService service)
		{
			Debug.Assert(HasContext());
			Debug.Assert(Instance == null);
			if (Instance == null || !Instance.Equals(service))
				return false;
			Instance = default;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService(TService service)
		{
			Debug.Assert(HasContext());
			var result = Instance;
			Instance = service;
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister()
		{
			Debug.Assert(HasContext());
			Instance = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister(TService service)
		{
			Debug.Assert(HasContext());
			if (Instance == null || !Instance.Equals(service))
				return;
			Instance = default;
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