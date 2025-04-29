using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;

namespace Sapientia.Messaging
{
	public delegate bool Filter<T>(in T msg);

	/// <summary>
	/// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
	/// </summary>
	internal sealed partial class MessengerHub : AsyncClass
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Receiver<TMessage> receiver) where TMessage : struct
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Receiver<TMessage> receiver, bool useStrongReferences)
			where TMessage : struct
		{
			return AddSubscriptionInternal<TMessage>(receiver, null, useStrongReferences);
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Receiver<TMessage> receiver, Filter<TMessage>? filter)
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Receiver<TMessage> receiver, Filter<TMessage>? filter,
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Unsubscribe<TMessage>(MessageSubscriptionToken<TMessage> subscriptionToken) where TMessage : struct
		{
			RemoveSubscriptionInternal(subscriptionToken);
		}

		/// <summary>
		/// Unsubscribe all subscribers from a particular message type.
		/// </summary>
		/// <param name="subscriptionToken">Subscription token received from Subscribe</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UnsubscribeAll<TMessage>() where TMessage : struct
		{
			RemoveAllSubscriptionInternal<TMessage>();
		}

		/// <summary>
		/// Publish a message to any subscribers
		/// </summary>
		/// <typeparam name="TMessage">Type of message</typeparam>
		/// <param name="msg">Message to deliver</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Send<TMessage>(ref TMessage msg)
			where TMessage : struct
		{
			SendInternal(ref msg);
		}

		/// <summary>
		/// Publish a message to any subscribers
		/// </summary>
		/// <typeparam name="TMessage">Type of message</typeparam>
		/// <param name="msg">Message to deliver</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SendAndUnsubscribeAll<TMessage>(ref TMessage msg)
			where TMessage : struct
		{
			SendAndUnsubscribeAllInternal(ref msg);
		}

		private MessageSubscriptionToken<TMessage> AddSubscriptionInternal<TMessage>(Receiver<TMessage> receiver,
			Filter<TMessage>? filter,
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
				subscriptionGroup.tokenToSubscription = new();
				_typeToSubscriptionGroup[messageType] = subscriptionGroup;
			}

			subscriptionGroup.tokenToSubscription.Add(subscriptionToken, subscription);

			return subscriptionToken;
		}

		private void RemoveSubscriptionInternal<TMessage>(MessageSubscriptionToken<TMessage> subscriptionToken) where TMessage : struct
		{
			if (subscriptionToken == null)
				throw new ArgumentNullException(nameof(subscriptionToken));

			using var scope = GetBusyScope();

			if (!_typeToSubscriptionGroup.TryGetValue(typeof(TMessage), out var subscriptionGroup))
				return;

			subscriptionGroup.tokenToSubscription.Remove(subscriptionToken);
		}

		private void RemoveAllSubscriptionInternal<TMessage>() where TMessage : struct
		{
			using var scope = GetBusyScope();

			if (!_typeToSubscriptionGroup.TryGetValue(typeof(TMessage), out var subscriptionGroup))
				return;

			subscriptionGroup.tokenToSubscription.Clear();
		}

		private void SendInternal<TMessage>(ref TMessage msg)
			where TMessage : struct
		{
			using var scope = GetBusyScope();

			if (!_typeToSubscriptionGroup.TryGetValue(typeof(TMessage), out var subscriptionGroup))
				return;

			// Создаётся буфер, т.к. при вызове Deliver получатель может изменить список подписок.
			// Сейчас GetBusyScope не блокирует поток если вызов происходит в текущем потоке.
			// Если жёстко запретить изменять список во время Deliver, то можно поймать deadlock.
			// SimpleList внутри использует пуллинг, так что лишняя память утекать не будет.
			var subscriptions = new SimpleList<IMessageSubscription>(subscriptionGroup.tokenToSubscription.Values);
			foreach (var subscription in subscriptions)
			{
				subscription.Deliver(ref msg);
			}
		}

		private void SendAndUnsubscribeAllInternal<TMessage>(ref TMessage msg)
			where TMessage : struct
		{
			using var scope = GetBusyScope();

			if (!_typeToSubscriptionGroup.TryGetValue(typeof(TMessage), out var subscriptionGroup))
				return;

			// Создаётся буфер, т.к. при вызове Deliver получатель может изменить список подписок.
			// Сейчас GetBusyScope не блокирует поток если вызов происходит в текущем потоке.
			// Если жёстко запретить изменять список во время Deliver, то можно поймать deadlock.
			// SimpleList внутри использует пуллинг, так что лишняя память утекать не будет.
			var subscriptions = new SimpleList<IMessageSubscription>(subscriptionGroup.tokenToSubscription.Values);
			subscriptionGroup.tokenToSubscription.Clear();

			foreach (var subscription in subscriptions)
			{
				subscription.Deliver(ref msg);
			}
		}
	}
}
