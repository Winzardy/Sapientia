using System.Runtime.CompilerServices;

namespace Sapientia.Messaging
{
	public class Messenger : StaticWrapper<MessengerHub>
	{
		private static MessengerHub hub
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance;
		}

		/// <summary>
		/// "Разослать" сообщение подписчикам <see cref="Subscribe{TMessage}(Receiver{TMessage})"/>
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SendAndUnsubscribeAll<TMessage>(ref TMessage msg) where TMessage : struct
			=> hub.SendAndUnsubscribeAll(ref msg);

		/// <summary>
		/// "Разослать" сообщение подписчикам <see cref="Subscribe{TMessage}(Receiver{TMessage})"/>
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send<TMessage>(ref TMessage msg) where TMessage : struct
			=> hub.Send(ref msg);

		/// <summary>
		/// Подписаться на сообщения
		/// </summary>
		/// <param name="receiver">Метод который вызовут при паблише сообщений</param>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Возвращает токен по которому нужно отписаться</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMessageSubscriptionToken Subscribe<TMessage>(Receiver<TMessage> receiver)
			where TMessage : struct
			=> hub.Subscribe(receiver);

		/// <summary>
		/// Подписаться на сообщения с фильтром
		/// </summary>
		/// <param name="receiver">Метод который вызовут при паблише сообщений</param>
		/// <param name="filter">Фильтр над сообщениями, если сообщение не подходит по условия метод получения не вызовется</param>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Возвращает токен по которому нужно отписаться</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMessageSubscriptionToken Subscribe<TMessage>(Receiver<TMessage> receiver, Filter<TMessage> filter)
			where TMessage : struct
			=> hub.Subscribe(receiver, filter);

		/// <summary>
		/// Отписывает всех подписчиков от сообщения
		/// </summary>
		/// <typeparam name="TMessage"></typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnsubscribeAll<TMessage>() where TMessage : struct
			=> hub.UnsubscribeAll<TMessage>();
	}
}
