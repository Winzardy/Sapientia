using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;

namespace Sapientia.ServiceManagement
{
	public static class ServiceLocator<TContext, TService>
	{
		private struct ContextSubscriber : IContextSubscriber<TContext>
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void IContextSubscriber<TContext>.SetContext(in TContext context)
			{
				ServiceLocator<TContext, TService>.SetContext(context);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void IContextSubscriber<TContext>.ReplaceContext(in TContext oldContext, in TContext newContext)
			{
				ServiceLocator<TContext, TService>.ReplaceContext(oldContext, newContext);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void IContextSubscriber<TContext>.RemoveContext(in TContext oldContext)
			{
				ServiceLocator<TContext, TService>.RemoveContext(oldContext);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void IContextSubscriber<TContext>.RemoveAllContext()
			{
				ServiceLocator<TContext, TService>.RemoveAllContext();
			}
		}

		/// <summary>
		/// For cases if TContext is a class
		/// </summary>
		private struct ContextContainer : IEquatable<ContextContainer>
		{
			public TContext context;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Equals(ContextContainer other)
			{
				return Equals(context, other.context);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static implicit operator ContextContainer(in TContext context)
			{
				return new ContextContainer { context = context };
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static implicit operator TContext(in ContextContainer contextContainer)
			{
				return contextContainer.context;
			}
		}

		private static readonly AsyncClass _asyncClass = new ();
		private static readonly Dictionary<ContextContainer, TService> _contextToService = new ();

		private static TService _currentService = default;
		private static ContextContainer _currentContext = default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static ServiceLocator()
		{
			ServiceContext<TContext>.AddSubscriber<ContextSubscriber>();
			SetContext(ServiceContext<TContext>.CurrentContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SetContext(in TContext context)
		{
			using var scope = _asyncClass.GetBusyScope();
			_contextToService.TryGetValue(context, out var service);
			_contextToService[_currentContext] = _currentService;
			_currentService = service;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveContext(in TContext oldContext)
		{
			using var scope = _asyncClass.GetBusyScope();
			_contextToService.Remove(oldContext);

			if (Equals(_currentContext.context, oldContext))
			{
				_currentContext = default;
				_contextToService.TryGetValue(_currentContext, out _currentService);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasContext(in TContext context)
		{
			using var scope = _asyncClass.GetBusyScope();
			return _contextToService.ContainsKey(context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReplaceContext(in TContext oldContext, in TContext newContext)
		{
			using var scope = _asyncClass.GetBusyScope();
			_contextToService.Remove(oldContext, out var value);
			_contextToService[newContext] = value;

			if (Equals(_currentContext.context, oldContext))
			{
				_currentContext = newContext;
				_currentService = value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryReplaceContext(in TContext oldContext, in TContext newContext)
		{
			using var scope = _asyncClass.GetBusyScope();
			if (!_contextToService.Remove(oldContext, out var value))
				return false;
			_contextToService[newContext] = value;

			if (Equals(_currentContext.context, oldContext))
			{
				_currentContext = newContext;
				_currentService = value;
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void RemoveAllContext()
		{
			using var scope = _asyncClass.GetBusyScope();

			_contextToService.Clear();
			_currentContext = default;
			_currentService = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetService<T>(in TContext context, in T service) where T: TService
		{
			using var scope = _asyncClass.GetBusyScope();
			if (Equals(_currentContext.context, context))
			{
				_currentService = service;
			}
			else
				_contextToService[context] = service;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetService<T>(in T service) where T: TService
		{
			using var scope = _asyncClass.GetBusyScope();
			_currentService = service;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRemoveService<T>(in TContext context, in T service) where T: TService
		{
			using var scope = _asyncClass.GetBusyScope();
			if (Equals(_currentContext.context, context))
			{
				if (Equals(_currentService, service))
				{
					_currentService = default;
					return true;
				}
			}
			else if (_contextToService.TryGetValue(context, out var currentService) && Equals(currentService, service))
				_contextToService[context] = default;

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService(TContext context)
		{
			using var scope = _asyncClass.GetBusyScope();
			return Equals(_currentContext.context, context) || _contextToService.ContainsKey(context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetService(TContext context, out TService service)
		{
			using var scope = _asyncClass.GetBusyScope();
			if (Equals(_currentContext.context, context))
			{
				service = _currentService;
				return true;
			}
			if (_contextToService.TryGetValue(context, out var value))
			{
				service = value;
				return true;
			}
			service = default;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService GetService()
		{
			using var scope = _asyncClass.GetBusyScope();
			return _currentService;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService GetService(TContext context)
		{
			using var scope = _asyncClass.GetBusyScope();
			if (Equals(_currentContext.context, context))
				return _currentService;
			return _contextToService.GetValueOrDefault(context);
		}

		public static bool TryGetServiceScope(TContext context, out ServiceScope<TService> serviceScope)
		{
			using var scope = _asyncClass.GetBusyScope();
			if (Equals(_currentContext.context, context))
			{
				serviceScope = new ServiceScope<TService>(_asyncClass, _currentService);
				return true;
			}
			if (_contextToService.TryGetValue(context, out var value))
			{
				serviceScope = new ServiceScope<TService>(_asyncClass, value);
				return true;
			}
			serviceScope = default;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceScope<TService> GetServiceScope()
		{
			using var scope = _asyncClass.GetBusyScope();
			return new ServiceScope<TService>(_asyncClass, _currentService);;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceScope<TService> GetServiceScope(TContext context)
		{
			using var scope = _asyncClass.GetBusyScope();
			if (Equals(_currentContext.context, context))
			{
				return new ServiceScope<TService>(_asyncClass, _currentService);
			}
			if (_contextToService.TryGetValue(context, out var value))
			{
				return new ServiceScope<TService>(_asyncClass, value);
			}
			return default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServicesEnumerable GetServicesEnumerable() => default;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServicesEnumerator GetServicesEnumerator() => ServicesEnumerator.Create();

		public struct ServicesEnumerable : IEnumerable<TService>
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public IEnumerator<TService> GetEnumerator()
			{
				return ServicesEnumerator.Create();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public struct ServicesEnumerator : IEnumerator<TService>
		{
			private SimpleList<TService>.Enumerator _enumerator;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static ServicesEnumerator Create()
			{
				using var scope = _asyncClass.GetBusyScope();
				return new ServicesEnumerator
				{
					_enumerator = new SimpleList<TService>(_contextToService.Values).GetEnumerator(),
				};
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				_enumerator.Reset();
			}

			public TService Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _enumerator.Current;
			}

			object IEnumerator.Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Current;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				_enumerator.Dispose();
			}
		}
	}

	public static partial class ServiceLocator<TService>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetService<TContext>(in TService service)
		{
			ServiceLocator<TContext, TService>.SetService(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetService<TContext>(in TContext context, in TService service)
		{
			ServiceLocator<TContext, TService>.SetService(context, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<TContext>(in TContext context, in TService service)
		{
			ServiceLocator<TContext, TService>.TryRemoveService(context, service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService GetService<TContext>()
		{
			return ServiceLocator<TContext, TService>.GetService();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService GetService<TContext>(in TContext context)
		{
			return ServiceLocator<TContext, TService>.GetService(context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetService<TContext>(in TContext context, out TService service)
		{
			return ServiceLocator<TContext, TService>.TryGetService(context, out service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasService<TContext>(in TContext context)
		{
			return ServiceLocator<TContext, TService>.HasService(context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReplaceContext<TContext>(in TContext oldContext, in TContext newContext)
		{
			ServiceLocator<TContext, TService>.ReplaceContext(oldContext, newContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryReplaceContext<TContext>(in TContext oldContext, in TContext newContext)
		{
			return ServiceLocator<TContext, TService>.TryReplaceContext(oldContext, newContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasContext<TContext>(in TContext context)
		{
			return ServiceLocator<TContext, TService>.HasContext(context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceScope<TService> GetServiceScope<TContext>(in TContext context)
		{
			return ServiceLocator<TContext, TService>.GetServiceScope(context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceScope<TService> GetServiceScope<TContext>()
		{
			return ServiceLocator<TContext, TService>.GetServiceScope();
		}
	}

	public readonly ref struct ServiceScope<TService>
	{
		private readonly AsyncClassBusyScope _busyScope;
		public readonly TService service;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ServiceScope(AsyncClass _asyncClass, in TService service)
		{
			_busyScope = _asyncClass.GetBusyScope();
			this.service = service;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_busyScope.Dispose();
		}
	}
}
