using System;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	public class CallbackService<TContext> : IService
	{
		private readonly DelayableAction<TContext> _callbackContextEvent = new ();
		private readonly DelayableAction _callbackEvent = new ();

		private static AsyncValueClassAsyncBusyScope<CallbackService<TContext>> GetServiceScope<TServiceContext>(TServiceContext serviceContext)
		{
			return ServiceLocator<TServiceContext, CallbackService<TContext>>.GetServiceBusyScope<CallbackService<TContext>>(serviceContext);
		}

		public static void InvokeDelayed<TServiceContext>(TServiceContext serviceContext)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.Value._callbackContextEvent.InvokeDelayedInterlocked();
			scope.Value._callbackEvent.InvokeDelayedInterlocked();
		}

		public static void InvokeDelayedOnce<TServiceContext>(TServiceContext serviceContext)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.Value._callbackContextEvent.InvokeDelayedOnceInterlocked();
			scope.Value._callbackEvent.InvokeDelayedOnceInterlocked();
		}

		public static void ReplaceServiceContext<TServiceContext>(TServiceContext oldContext, TServiceContext newContext, ServiceAccessType accessType = ServiceAccessType.Default)
		{
			ServiceLocator<TServiceContext, CallbackService<TContext>>.ReplaceContext<CallbackService<TContext>>(oldContext, newContext);
			ServiceLocator<TServiceContext, CallbackService<TContext>>.SetAccessType(newContext, accessType);
		}

		public static void DelayInvoke<TServiceContext>(TServiceContext serviceContext, in TContext context = default)
		{
#if !DEBUG
			try
#endif
			{
				using var scope = GetServiceScope(serviceContext);
				var service = scope.Value;
				if (service._callbackContextEvent.HasSubscribers())
					service._callbackContextEvent.DelayInvokeInterlocked(context);
				if (service._callbackEvent.HasSubscribers())
					service._callbackEvent.DelayInvokeInterlocked();
			}
#if !DEBUG
			catch
			{
				// ignored
			}
#endif
		}


		public static void Subscribe<TServiceContext>(TServiceContext serviceContext, Action<TContext> callback)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.Value._callbackContextEvent.SubscribeInterlocked(callback);
		}

		public static void UnSubscribe<TServiceContext>(TServiceContext serviceContext, Action<TContext> callback)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.Value._callbackContextEvent.UnSubscribeInterlocked(callback);
		}

		public static void Subscribe<TServiceContext>(TServiceContext serviceContext, Action callback)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.Value._callbackEvent.SubscribeInterlocked(callback);
		}

		public static void UnSubscribe<TServiceContext>(TServiceContext serviceContext, Action callback)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.Value._callbackEvent.UnSubscribeInterlocked(callback);
		}
	}
}