using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	public static class ServiceLocator<TContext, TService> where TService : IService
	{
		private static readonly Dictionary<TContext, TService> CONTEXT_TO_SERVICE = new();

		[AllowNull]
		public static TContext Context { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

		[AllowNull]
		public static TService Instance { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

		public static void SetContext<T>(T newContext) where T: TContext
		{
			if (Context != null)
				CONTEXT_TO_SERVICE[Context] = Instance;

			Context = newContext;
			if (CONTEXT_TO_SERVICE.TryGetValue(newContext, out var service))
				Instance = service;
			else
				Instance = default;
		}

		public static bool HasContext()
		{
			return Context != null;
		}

		public static bool HasInstance()
		{
			return Instance != null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Get<T>() where T: TService, new()
		{
			Debug.Assert(Context != null);
			if (Instance == null)
				return Create<T>();
			return Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGet(out TService service)
		{
			Debug.Assert(Context != null);
			if (Instance == null)
			{
				service = default;
				return false;
			}
			service = Instance;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<T>() where T: TService, new()
		{
			Debug.Assert(Context != null);
			var value = new T();
			Register(value);
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRegister(TService service)
		{
			Debug.Assert(Context != null);
			if (Instance != null)
				return false;
			Instance = service;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Register(TService service)
		{
			Debug.Assert(Context != null);
			Debug.Assert(Instance == null);
			Instance = service;

			return Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryUnRegister(TService service)
		{
			Debug.Assert(Context != null);
			if (Instance == null || !Instance.Equals(service))
				return false;
			Instance = default;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService(TService service)
		{
			Debug.Assert(Context != null);
			var result = Instance;
			Instance = service;
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister()
		{
			Debug.Assert(Context != null);
			Instance = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister(TService service)
		{
			Debug.Assert(Context != null);
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
			if (Instance == null)
			{
				service = default;
				return false;
			}
			service = Instance;
			return true;
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

		#endregion
	}

	public interface IService {}
}