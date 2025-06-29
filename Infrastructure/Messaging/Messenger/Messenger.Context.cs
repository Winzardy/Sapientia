﻿using System.Runtime.CompilerServices;
using Sapientia;
using Sapientia.ServiceManagement;

namespace Messaging
{
	public static class Messenger<TContext>
	{
		/// <summary>
		/// "Разослать" сообщение подписчикам <see cref="Subscribe{TMessage}(System.Action{TMessage})"/>
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send<TMessage>(in TContext context, ref TMessage msg)
			where TMessage : struct
		{
			if (ServiceLocator<TContext, MessageBus>.TryGetService(context, out var messengerHub))
				messengerHub.Send(ref msg);
		}

		/// <summary>
		/// Подписаться на сообщения
		/// </summary>
		/// <param name="receiver">Метод который вызовут при паблише сообщений</param>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Возвращает токен по которому нужно отписаться</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMessageSubscriptionToken Subscribe<TMessage>(in TContext context, Receiver<TMessage> receiver)
			where TMessage : struct =>
			ServiceContext<TContext>.GetOrCreateService<MessageBus>(context).Subscribe(receiver);

		/// <summary>
		/// Подписаться на сообщения с фильтром
		/// </summary>
		/// <param name="receiver">Метод который вызовут при паблише сообщений</param>
		/// <param name="filter">Фильтр над сообщениями, если сообщение не подходит по условия метод получения не вызовется</param>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Возвращает токен по которому нужно отписаться</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMessageSubscriptionToken Subscribe<TMessage>(in TContext context, Receiver<TMessage> receiver,
			Filter<TMessage> filter)
			where TMessage : struct =>
			ServiceContext<TContext>.GetOrCreateService<MessageBus>(context).Subscribe(receiver, filter);

		/// <summary>
		/// Отписывает всех подписчиков от сообщения
		/// </summary>
		/// <typeparam name="TMessage"></typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnsubscribeAll<TMessage>(in TContext context)
			where TMessage : struct =>
			ServiceContext<TContext>.GetOrCreateService<MessageBus>(context).UnsubscribeAll<TMessage>();
	}
}
