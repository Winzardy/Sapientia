using System;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	public class CallbackService<TContext> : IService
	{
		public event Action<TContext> CallbackContextEvent;
		public event Action CallbackEvent;

		private static CallbackService<TContext> Service => ServiceLocator.Get<CallbackService<TContext>>();

		public static void ExecuteCallback(in TContext context = default)
		{
#if !DEBUG
			try
#endif
			{
				var service = Service;
				service.CallbackContextEvent?.Invoke(context);
				service.CallbackEvent?.Invoke();
			}
#if !DEBUG
			catch
			{
				// ignored
			}
#endif
		}

		public static void Subscribe(Action<TContext> callback)
		{
			Service.CallbackContextEvent += callback;
		}

		public static void UnSubscribe(Action<TContext> callback)
		{
			Service.CallbackContextEvent -= callback;
		}

		public static void Subscribe(Action callback)
		{
			Service.CallbackEvent += callback;
		}

		public static void UnSubscribe(Action callback)
		{
			Service.CallbackEvent -= callback;
		}
	}
}