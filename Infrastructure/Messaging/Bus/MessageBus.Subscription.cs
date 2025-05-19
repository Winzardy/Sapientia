using System;
using Sapientia.Extensions;

namespace Sapientia.Messaging
{
	public sealed partial class MessageBus
	{
		/// <summary>
		/// Представляет подписку на сообщение
		/// </summary>
		public interface IMessageSubscription
		{
			/// <summary>
			/// Доставить сообщение
			/// </summary>
			/// <param name="message">Сообщение, подлежащее доставке</param>
			public void Deliver<T>(ref T message) where T : struct;
		}

		private class StrongMessageSubscription<TMessage> : IMessageSubscription where TMessage : struct
		{
			private readonly Receiver<TMessage> _receiver;
			private readonly Filter<TMessage> _filter;

			public StrongMessageSubscription(Receiver<TMessage> receiver, Filter<TMessage> filter)
			{
				_receiver = receiver;
				_filter = filter;
			}

			public void Deliver<T>(ref T rawMessage) where T : struct
			{
				ref var message = ref rawMessage.As<T, TMessage>();
				if (_filter != null && !_filter.Invoke(message))
					return;
				_receiver.Invoke(message);
			}
		}

		private class WeakMessageSubscription<TMessage> : IMessageSubscription where TMessage : struct
		{
			private readonly WeakReference<Receiver<TMessage>> _weakReceiver;
			private readonly WeakReference<Filter<TMessage>> _weakFilter;

			public WeakMessageSubscription(Receiver<TMessage> receiver, Filter<TMessage> filter)
			{
				_weakReceiver = new(receiver);
				_weakFilter = filter == null ? null : new(filter);
			}

			public void Deliver<T>(ref T rawMessage) where T : struct
			{
				if (!_weakReceiver.TryGetTarget(out var receiver))
					return;

				ref var message = ref rawMessage.As<T, TMessage>();
				if (_weakFilter != null && (!_weakFilter.TryGetTarget(out var filter) || !filter.Invoke(message)))
					return;

				receiver.Invoke(message);
			}
		}
	}
}
