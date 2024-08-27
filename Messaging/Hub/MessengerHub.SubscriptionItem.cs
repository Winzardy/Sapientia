using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sapientia.Extensions;

namespace Messaging
{
	/// <summary>
	/// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
	/// </summary>
	public sealed partial class MessengerHub
	{
		/// <summary>
		/// Represents a message subscription
		/// </summary>
		public interface IMessageSubscription
		{
			/// <summary>
			/// Deliver the message
			/// </summary>
			/// <param name="message">Message to deliver</param>
			public void Deliver<T>(ref T message) where T : struct;
		}

		private struct StrongMessageSubscription<TMessage> : IMessageSubscription where TMessage : struct
		{
			private Action<TMessage> _receiver;

			[CanBeNull]
			private Func<TMessage, bool> _filter;

			public StrongMessageSubscription(Action<TMessage> receiver, [CanBeNull] Func<TMessage, bool> filter)
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

		private struct WeakMessageSubscription<TMessage> : IMessageSubscription where TMessage : struct
		{
			private WeakReference<Action<TMessage>> _weakReceiver;

			[CanBeNull]
			private WeakReference<Func<TMessage, bool>> _weakFilter;

			public WeakMessageSubscription(Action<TMessage> receiver, [CanBeNull] Func<TMessage, bool> filter)
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

		private struct SubscriptionGroup
		{
			public Dictionary<IMessageSubscriptionToken, IMessageSubscription> tokenToSubscription;
		}
	}
}
