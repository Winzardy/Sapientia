using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.Extensions
{
	public static class ServiceLocator<TContext, TService>
	{
		private delegate AsyncValueClassAsyncBusyScope<TService> GetScope(AsyncValueClass<TService> instance);
		private delegate TService GetInstance(AsyncValueClass<TService> instance);
		private delegate void SetInstance(AsyncValueClass<TService> instance, TService service);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static AsyncValueClassAsyncBusyScope<TService> InterlockedGetScope(AsyncValueClass<TService> instance)
		{
			return instance.GetAsyncBusyScope();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TService InterlockedGet(AsyncValueClass<TService> instance)
		{
			return instance.ReadValue();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void InterlockedSet(AsyncValueClass<TService> instance, TService service)
		{
			instance.SetValue(service);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static AsyncValueClassAsyncBusyScope<TService> DefaultGetScope(AsyncValueClass<TService> instance)
		{
			return instance.GetAsyncBusyScope(true);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TService DefaultGet(AsyncValueClass<TService> instance)
		{
			return instance.value;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void DefaultSet(AsyncValueClass<TService> instance, TService service)
		{
			instance.value = service;
		}

		private static readonly Dictionary<TContext, (GetScope getScope, GetInstance getInstance, SetInstance setInstance, AsyncValueClass<TService> instance)> SERVICES = new ();

		private static (GetScope getScope, GetInstance getInstance, SetInstance setInstance, AsyncValueClass<TService> instance) GetContextValue<T>(TContext context) where T: TService, new()
		{
			if (!SERVICES.TryGetValue(context, out var value))
			{
				value = (DefaultGetScope, DefaultGet, DefaultSet, new AsyncValueClass<TService>(new T()));
				SERVICES[context] = value;
			}
			return value;
		}

		private static (GetScope getScope, GetInstance getInstance, SetInstance setInstance, AsyncValueClass<TService> instance) GetContextValue(TContext context)
		{
			if (!SERVICES.TryGetValue(context, out var value))
			{
				value = (DefaultGetScope, DefaultGet, DefaultSet, new AsyncValueClass<TService>(default));
				SERVICES[context] = value;
			}
			return value;
		}

		public static void SetAccessType(TContext context, ServiceAccessType accessType)
		{
			GetScope getScope;
			GetInstance getInstance;
			SetInstance setInstance;
			switch (accessType)
			{
				case ServiceAccessType.Interlocked:
				{
					getScope = InterlockedGetScope;
					getInstance = InterlockedGet;
					setInstance = InterlockedSet;
					break;
				}
				default:
				{
					getScope = DefaultGetScope;
					getInstance = DefaultGet;
					setInstance = DefaultSet;
					break;
				}
			}

			if (!SERVICES.TryGetValue(context, out var value))
				value.instance = new AsyncValueClass<TService>(default);
			value.getScope = getScope;
			value.getInstance = getInstance;
			value.setInstance = setInstance;
			SERVICES[context] = value;
		}

		public static void SetContext(TContext context)
		{
			GetContextValue(context);
		}

		public static void ReplaceContext<T>(TContext oldContext, TContext newContext) where T: TService, new()
		{
			if (SERVICES.ContainsKey(newContext))
			{
				RemoveContext(oldContext);
			}
			else if (oldContext != null && SERVICES.TryGetValue(oldContext, out var value))
			{
				SERVICES[newContext] = value;
				SERVICES.Remove(oldContext);
			}
			else
				SetService(newContext, new T());
		}

		public static void RemoveContext(TContext context)
		{
			SERVICES.Remove(context);
		}

		public static void ResetContext()
		{
			SERVICES.Clear();
		}

		public static void SetService(TContext context, TService service)
		{
			if (SERVICES.TryGetValue(context, out var value))
			{
				value.setInstance(value.instance, service);
			}
			else
			{
				value = (DefaultGetScope, DefaultGet, DefaultSet, new AsyncValueClass<TService>(service));
				SERVICES[context] = value;
			}
		}

		public static bool HasService(TContext context)
		{
			return SERVICES.ContainsKey(context);
		}

		public static bool TryReadService(TContext context, out TService service)
		{
			if (SERVICES.TryGetValue(context, out var value))
			{
				service = value.getInstance.Invoke(value.instance);
				return true;
			}
			service = default;
			return false;
		}

		public static TService ReadService(TContext context)
		{
			if (SERVICES.TryGetValue(context, out var value))
				return value.getInstance.Invoke(value.instance);
			return default;
		}

		public static TService GetService<T>(TContext context) where T: TService, new()
		{
			var value = GetContextValue<T>(context);
			return value.getInstance.Invoke(value.instance);
		}

		public static bool TryReadServiceBusyScope(TContext context, out AsyncValueClassAsyncBusyScope<TService> scope)
		{
			if (SERVICES.TryGetValue(context, out var value))
			{
				scope = value.getScope.Invoke(value.instance);
				return true;
			}
			scope = default;
			return false;
		}

		public static AsyncValueClassAsyncBusyScope<TService> ReadServiceBusyScope(TContext context)
		{
			if (SERVICES.TryGetValue(context, out var value))
				return value.getScope.Invoke(value.instance);
			return default;
		}

		public static AsyncValueClassAsyncBusyScope<TService> GetServiceBusyScope<T>(TContext context) where T: TService, new()
		{
			var value = GetContextValue<T>(context);
			return value.getScope.Invoke(value.instance);
		}

		public static ServicesEnumerator GetServicesEnumerator() => ServicesEnumerator.Create();

		public struct ServicesEnumerator : IEnumerator<TService>
		{
			private IEnumerator<KeyValuePair<TContext, (GetScope getScope, GetInstance getInstance, SetInstance setInstance,
				AsyncValueClass<TService> instance)>> _enumerator;

			public static ServicesEnumerator Create()
			{
				return new ServicesEnumerator
				{
					_enumerator = SERVICES.GetEnumerator(),
				};
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator.Reset();
			}

			public TService Current
			{
				get
				{
					var value = _enumerator.Current.Value;
					return value.getInstance(value.instance);
				}
			}

			object IEnumerator.Current => Current;

			public void Dispose()
			{
				_enumerator.Dispose();
			}
		}
	}
}