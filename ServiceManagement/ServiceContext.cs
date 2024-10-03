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
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddSubscriber<T>() where T: IContextSubscriber<TContext>, new()
		{
			using var scope = _asyncClass.GetBusyScope();
			_subscribers.Add(new T());
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
		public static void RemoveAllContext()
		{
			using var scope = _asyncClass.GetBusyScope();
			_currentContext = default;

			foreach (var subscriber in _subscribers)
			{
				subscriber.RemoveAllContext();
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

	public interface IContextSubscriber<TContext>
	{
		public void SetContext(in TContext context);
		public void ReplaceContext(in TContext oldContext, in TContext newContext);
		public void RemoveContext(in TContext oldContext);
		public void RemoveAllContext();
	}
}
