﻿using System.Runtime.CompilerServices;
using Sapientia.ServiceManagement;

namespace Sapientia.Messaging
{
	public class Messenger
	{
		public static void Initialize()
		{
			Terminate();
			ServiceLocator<MessengerHub>.Create<MessengerHub>();
		}

		public static void Terminate()
		{
			ServiceLocator<MessengerHub>.UnRegister();
			ServiceLocator<MessengerHub>.RemoveAllContext();
		}

		/// <summary>
		/// "Разослать" сообщение подписчикам <see cref="Subscribe{TMessage}(System.Action{TMessage})"/>
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SendAndUnsubscribeAll<TMessage>(ref TMessage msg)
			where TMessage : struct =>
			ServiceLocator<MessengerHub>.Instance.SendAndUnsubscribeAll(ref msg);

		/// <summary>
		/// "Разослать" сообщение подписчикам <see cref="Subscribe{TMessage}(System.Action{TMessage})"/>
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send<TMessage>(ref TMessage msg)
			where TMessage : struct
		{
			ServiceLocator<MessengerHub>.Instance.Send(ref msg);
		}

		/// <summary>
		/// Подписаться на сообщения
		/// </summary>
		/// <param name="receiver">Метод который вызовут при паблише сообщений</param>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Возвращает токен по которому нужно отписаться</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMessageSubscriptionToken Subscribe<TMessage>(Receiver<TMessage> receiver)
			where TMessage : struct =>
			ServiceLocator<MessengerHub>.Instance.Subscribe(receiver);

		/// <summary>
		/// Подписаться на сообщения с фильтром
		/// </summary>
		/// <param name="receiver">Метод который вызовут при паблише сообщений</param>
		/// <param name="filter">Фильтр над сообщениями, если сообщение не подходит по условия метод получения не вызовется</param>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Возвращает токен по которому нужно отписаться</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMessageSubscriptionToken Subscribe<TMessage>(Receiver<TMessage> receiver,
			Filter<TMessage> filter)
			where TMessage : struct =>
			ServiceLocator<MessengerHub>.Instance.Subscribe(receiver, filter);

		/// <summary>
		/// Отписывает всех подписчиков от сообщения
		/// </summary>
		/// <typeparam name="TMessage"></typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnsubscribeAll<TMessage>() where TMessage : struct => ServiceLocator<MessengerHub>.Instance.UnsubscribeAll<TMessage>();
	}
}
