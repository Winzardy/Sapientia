using System;
using Sapientia.Extensions;
using Sapientia.ServiceManagement;

namespace Sapientia.Data
{
	public class CallbackService<TContext>
	{
		private readonly DelayableAction<TContext> _callbackContextEvent = new ();
		private readonly DelayableAction _callbackEvent = new ();

		private static ServiceScope<CallbackService<TContext>> GetServiceScope<TServiceContext>(TServiceContext serviceContext)
		{
			if (ServiceLocator<CallbackService<TContext>>.GetService(serviceContext) == null)
				ServiceLocator<CallbackService<TContext>>.SetService(serviceContext, new());
			return ServiceLocator<CallbackService<TContext>>.GetServiceScope(serviceContext);
		}

		public static void InvokeDelayed<TServiceContext>(TServiceContext serviceContext)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.service._callbackContextEvent.InvokeDelayedInterlocked();
			scope.service._callbackEvent.InvokeDelayedInterlocked();
		}

		public static void InvokeDelayedOnce<TServiceContext>(TServiceContext serviceContext)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.service._callbackContextEvent.InvokeDelayedOnceInterlocked();
			scope.service._callbackEvent.InvokeDelayedOnceInterlocked();
		}

		public static void ReplaceServiceContext<TServiceContext>(TServiceContext oldContext, TServiceContext newContext)
		{
			ServiceLocator<CallbackService<TContext>>.ReplaceContext(oldContext, newContext);
			var service = ServiceLocator<CallbackService<TContext>>.GetService(newContext);
			if (service == null)
				ServiceLocator<CallbackService<TContext>>.SetService(newContext, new());
		}

		public static void DelayInvoke<TServiceContext>(TServiceContext serviceContext, in TContext context = default)
		{
#if !DEBUG
			try
#endif
			{
				using var scope = GetServiceScope(serviceContext);
				var service = scope.service;
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
			scope.service._callbackContextEvent.SubscribeInterlocked(callback);
		}

		public static void UnSubscribe<TServiceContext>(TServiceContext serviceContext, Action<TContext> callback)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.service._callbackContextEvent.UnSubscribeInterlocked(callback);
		}

		public static void Subscribe<TServiceContext>(TServiceContext serviceContext, Action callback)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.service._callbackEvent.SubscribeInterlocked(callback);
		}

		public static void UnSubscribe<TServiceContext>(TServiceContext serviceContext, Action callback)
		{
			using var scope = GetServiceScope(serviceContext);
			scope.service._callbackEvent.UnSubscribeInterlocked(callback);
		}
	}
}
