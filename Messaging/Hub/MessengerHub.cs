using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sapientia.Data;

namespace Sapientia.Messaging
{
	/// <summary>
	/// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
	/// </summary>
	public sealed partial class MessengerHub : AsyncClass
	{
		private readonly Dictionary<Type, SubscriptionGroup> _typeToSubscriptionGroup = new();

		/// <summary>
		/// Subscribe to a message type with the given destination and receiver action.
		/// All references are held with strong references
		///
		/// All messages of this type will be delivered.
		/// </summary>
		/// <typeparam name="TMessage">Type of message</typeparam>
		/// <param name="receiver">Action to invoke when message is delivered</param>
		/// <returns>MessageSubscription used to unsubscribing</returns>
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Action<TMessage> receiver) where TMessage : struct
		{
			return AddSubscriptionInternal(receiver, null, true);
		}

		/// <summary>
		/// Subscribe to a message type with the given destination and receiver action.
		/// Messages will be delivered via the specified proxy.
		///
		/// All messages of this type will be delivered.
		/// </summary>
		/// <typeparam name="TMessage">Type of message</typeparam>
		/// <param name="receiver">Action to invoke when message is delivered</param>
		/// <param name="useStrongReferences">Use strong references to destination and receiver </param>
		/// <param name="proxy">Proxy to use when delivering the messages</param>
		/// <returns>MessageSubscription used to unsubscribing</returns>
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Action<TMessage> receiver, bool useStrongReferences)
			where TMessage : struct
		{
			return AddSubscriptionInternal<TMessage>(receiver, (m) => true, useStrongReferences);
		}

		/// <summary>
		/// Subscribe to a message type with the given destination and receiver action with the given filter.
		/// Messages will be delivered via the specified proxy.
		/// All references (apart from the proxy) are held with WeakReferences
		///
		/// Only messages that "pass" the filter will be delivered.
		/// </summary>
		/// <typeparam name="TMessage">Type of message</typeparam>
		/// <param name="receiver">Action to invoke when message is delivered</param>
		/// <returns>MessageSubscription used to unsubscribing</returns>
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Action<TMessage> receiver, [CanBeNull] Func<TMessage, bool> filter)
			where TMessage : struct
		{
			return AddSubscriptionInternal<TMessage>(receiver, filter, true);
		}

		/// <summary>
		/// Subscribe to a message type with the given destination and receiver action with the given filter.
		/// All references are held with WeakReferences
		///
		/// Only messages that "pass" the filter will be delivered.
		/// </summary>
		/// <typeparam name="TMessage">Type of message</typeparam>
		/// <param name="receiver">Action to invoke when message is delivered</param>
		/// <param name="useStrongReferences">Use strong references to destination and receiver </param>
		/// <returns>MessageSubscription used to unsubscribing</returns>
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Action<TMessage> receiver, [CanBeNull] Func<TMessage, bool> filter,
			bool useStrongReferences)
			where TMessage : struct
		{
			return AddSubscriptionInternal<TMessage>(receiver, filter, useStrongReferences);
		}

		/// <summary>
		/// Unsubscribe from a particular message type.
		///
		/// Does not throw an exception if the subscription is not found.
		/// </summary>
		/// <param name="subscriptionToken">Subscription token received from Subscribe</param>
		public void Unsubscribe<TMessage>(MessageSubscriptionToken<TMessage> subscriptionToken) where TMessage: struct
		{
			RemoveSubscriptionInternal(subscriptionToken);
		}

		/// <summary>
		/// Publish a message to any subscribers
		/// </summary>
		/// <typeparam name="TMessage">Type of message</typeparam>
		/// <param name="msg">Message to deliver</param>
		public void Send<TMessage>(ref TMessage msg)
			where TMessage : struct
		{
			SendInternal(ref msg);
		}

		private MessageSubscriptionToken<TMessage> AddSubscriptionInternal<TMessage>(Action<TMessage> receiver,
			[CanBeNull] Func<TMessage, bool> filter,
			bool strongReference)
			where TMessage : struct
		{
			if (receiver == null)
				throw new ArgumentNullException(nameof(receiver));

			using var scope = GetBusyScope();

			var messageType = typeof(TMessage);
			var subscriptionToken = new MessageSubscriptionToken<TMessage>(this);

			IMessageSubscription subscription;
			if (strongReference)
				subscription = new StrongMessageSubscription<TMessage>(receiver, filter);
			else
				subscription = new WeakMessageSubscription<TMessage>(receiver, filter);

			if (!_typeToSubscriptionGroup.TryGetValue(messageType, out var subscriptionGroup))
			{
				subscriptionGroup.tokenToSubscription = new ();
				_typeToSubscriptionGroup[messageType] = subscriptionGroup;
			}

			subscriptionGroup.tokenToSubscription.Add(subscriptionToken, subscription);


			return subscriptionToken;
		}

		private void RemoveSubscriptionInternal<TMessage>(MessageSubscriptionToken<TMessage> subscriptionToken) where TMessage: struct
		{
			if (subscriptionToken == null)
				throw new ArgumentNullException(nameof(subscriptionToken));

			using var scope = GetBusyScope();

			if (!_typeToSubscriptionGroup.TryGetValue(typeof(TMessage), out var subscriptionGroup))
				return;

			subscriptionGroup.tokenToSubscription.Remove(subscriptionToken);
		}

		private void SendInternal<TMessage>(ref TMessage msg)
			where TMessage : struct
		{
			using var scope = GetBusyScope();

			if (!_typeToSubscriptionGroup.TryGetValue(typeof(TMessage), out var subscriptionGroup))
				return;

			foreach (var (_, subscription) in subscriptionGroup.tokenToSubscription)
			{
				subscription.Deliver(ref msg);
			}
		}
	}
}
