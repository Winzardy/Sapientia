using System;
using Sapientia;

namespace Messaging
{
	public sealed partial class MessageBus
	{
		private static int _hubCounter;
		/// <summary>
		/// Only runtime hub index
		/// </summary>
		private readonly int _index;

		public MessageBus() =>
			_index = _hubCounter++;

		private MessageSubscriptionToken<TMessage> AddSubscriptionInternal<TMessage>(
			Receiver<TMessage> receiver,
			Filter<TMessage> filter,
			bool strongReference)
			where TMessage : struct
		{
			if (receiver == null)
				throw new ArgumentNullException(nameof(receiver));

			using var scope = GetBusyScope();
			var token = new MessageSubscriptionToken<TMessage>(_index);
			IMessageSubscription subscription;
			if (strongReference)
				subscription = new StrongMessageSubscription<TMessage>(receiver, filter);
			else
				subscription = new WeakMessageSubscription<TMessage>(receiver, filter);
			SubscriptionMap<TMessage>.Add(token, subscription);
			return token;
		}

		private void RemoveSubscriptionInternal<TMessage>(MessageSubscriptionToken<TMessage> subscriptionToken) where TMessage : struct
		{
			if (subscriptionToken == null)
				throw new ArgumentNullException(nameof(subscriptionToken));

			using var scope = GetBusyScope();
			SubscriptionMap<TMessage>.Remove(subscriptionToken);
		}

		private void RemoveAllSubscriptionInternal<TMessage>() where TMessage : struct
		{
			using var scope = GetBusyScope();
			SubscriptionMap<TMessage>.Clear(_index);
		}

		private void SendInternal<TMessage>(ref TMessage msg) where TMessage : struct
		{
			using var scope = GetBusyScope();
			SubscriptionMap<TMessage>.Deliver(_index, ref msg);
		}

		private void SendAndUnsubscribeAllInternal<TMessage>(ref TMessage msg) where TMessage : struct
		{
			using var scope = GetBusyScope();
			SubscriptionMap<TMessage>.Deliver(_index, ref msg, true);
		}
	}
}
