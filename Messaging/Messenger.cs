﻿using System;
using Sapientia.Extensions;

namespace Sapientia.Messaging
{
	public class Messenger : StaticWrapper<MessengerHub>
	{
		/// <summary>
		/// "Разослать" сообщение подписчикам <see cref="Subscribe{TMessage}(System.Action{TMessage})"/>
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		public static void Send<TMessage>(ref TMessage msg)
			where TMessage : struct =>
			instance.Send(ref msg);

		/// <summary>
		/// Подписаться на сообщения
		/// </summary>
		/// <param name="receiver">Метод который вызовут при паблише сообщений</param>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Возвращает токен по которому нужно отписаться</returns>
		public static IMessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> receiver)
			where TMessage : struct =>
			instance.Subscribe(receiver);

		/// <summary>
		/// Подписаться на сообщения с фильтром
		/// </summary>
		/// <param name="receiver">Метод который вызовут при паблише сообщений</param>
		/// <param name="filter">Фильтр над сообщениями, если сообщение не подходит по условия метод получения не вызовется</param>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Возвращает токен по которому нужно отписаться</returns>
		public static IMessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> receiver,
			Func<TMessage, bool> filter)
			where TMessage : struct =>
			instance.Subscribe(receiver, filter);
	}
}