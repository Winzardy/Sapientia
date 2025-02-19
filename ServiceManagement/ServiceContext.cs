using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;

namespace Sapientia.ServiceManagement
{
	public static class ServiceContext<TContext>
	{
		private static AsyncClass _asyncClass = new();

		private static SimpleList<IContextSubscriber<TContext>> _subscribers = new ();

		private static TContext _currentContext = default;

		public static TContext CurrentContext
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				using var scope = _asyncClass.GetBusyScope();
				return _currentContext;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddSubscriber<T>(in T subscriber) where T: IContextSubscriber<TContext>
		{
			using var scope = _asyncClass.GetBusyScope();
			_subscribers.Add(subscriber);
			ServiceLocator<T>.contextSubscribers.Add(subscriber);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddSubscriber<T>() where T: IContextSubscriber<TContext>, new()
		{
			using var scope = _asyncClass.GetBusyScope();

			var subscriber = new T();
			_subscribers.Add(subscriber);
			ServiceLocator<T>.contextSubscribers.Add(subscriber);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetContext(in TContext newContext)
		{
			using var scope = _asyncClass.GetBusyScope();
			if (_currentContext.Equals(newContext))
				return;
			_currentContext = newContext;

			foreach (var subscriber in _subscribers)
			{
				subscriber.SetContext(_currentContext);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReplaceContext(in TContext oldContext, in TContext newContext)
		{
			using var scope = _asyncClass.GetBusyScope();
			if (_currentContext.Equals(oldContext))
				_currentContext = newContext;

			foreach (var subscriber in _subscribers)
			{
				subscriber.ReplaceContext(oldContext, newContext);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveContext(in TContext oldContext)
		{
			using var scope = _asyncClass.GetBusyScope();
			if (_currentContext.Equals(oldContext))
				_currentContext = default;

			foreach (var subscriber in _subscribers)
			{
				subscriber.RemoveContext(oldContext);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveAllContext(bool dispose = false)
		{
			using var scope = _asyncClass.GetBusyScope();
			_currentContext = default;

			foreach (var subscriber in _subscribers)
			{
				subscriber.RemoveAllContext(dispose);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetService<TService>(in TService service)
		{
			ServiceLocator<TContext, TService>.SetService(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService GetService<TService>()
		{
			return ServiceLocator<TContext, TService>.GetService();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService GetOrCreateService<TService>(in TContext context) where TService: new()
		{
			if (!ServiceLocator<TContext, TService>.TryGetService(context, out var result))
			{
				result = new TService();
				ServiceLocator<TContext, TService>.SetService(context, result);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ContextScope GetContextScope(in TContext newContext)
		{
			SetContext(newContext);
			return new ContextScope(newContext);
		}

		public readonly ref struct ContextScope
		{
			private readonly TContext _previousContext;

			public ContextScope(in TContext previousContext)
			{
				_previousContext = previousContext;
			}

			public void Dispose()
			{
				SetContext(_previousContext);
			}
		}
	}

	public interface IContextSubscriber<TContext> : IContextSubscriber
	{
		public void SetContext(in TContext context);
		public void ReplaceContext(in TContext oldContext, in TContext newContext);
		public void RemoveContext(in TContext oldContext);
	}

	public interface IContextSubscriber
	{
		public void RemoveAllContext(bool dispose);
	}
}
