using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Sapientia;

namespace Messaging
{
	// TODO: этим можно пользоваться только на клиенте, на сервере многопоток...
	// Нужно вынести это в reference assembly для клиента, на сервере нужно напрямую с Bus
	// работать если вообще будет такая логика
	public class Messenger : StaticAccessor<MessageBus>
	{
		private static MessageBus bus
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance;
		}

		/// <summary>
		/// "Разослать" сообщение подписчикам <see cref="Subscribe{TMessage}(Receiver{T})"/>
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SendAndUnsubscribeAll<TMessage>(ref TMessage msg) where TMessage : struct
			=> bus.SendAndUnsubscribeAll(ref msg);

		/// <summary>
		/// "Разослать" сообщение подписчикам <see cref="Subscribe{TMessage}(Receiver{T})"/>
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Получил ли кто-то сообщение?</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Send<TMessage>(ref TMessage msg) where TMessage : struct
			=> bus.Send(ref msg);

		/// <summary>
		/// Подписаться на сообщения
		/// </summary>
		/// <param name="receiver">Метод который вызовут при паблише сообщений</param>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Возвращает токен по которому нужно отписаться</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[MustUseReturnValue("Возвращает токен по которому нужно отписаться")]
		public static IMessageSubscriptionToken Subscribe<TMessage>(Receiver<TMessage> receiver)
			where TMessage : struct
			=> bus.Subscribe(receiver);

		/// <summary>
		/// Подписаться на сообщения с фильтром
		/// </summary>
		/// <param name="receiver">Метод который вызовут при паблише сообщений</param>
		/// <param name="filter">Фильтр над сообщениями, если сообщение не подходит по условия метод получения не вызовется</param>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <returns>Возвращает токен по которому нужно отписаться</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[MustUseReturnValue("Возвращает токен по которому нужно отписаться")]
		public static IMessageSubscriptionToken Subscribe<TMessage>(Receiver<TMessage> receiver, Filter<TMessage> filter)
			where TMessage : struct
			=> bus.Subscribe(receiver, filter);

		/// <summary>
		/// Отписывает всех подписчиков от сообщения
		/// </summary>
		/// <typeparam name="TMessage"></typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnsubscribeAll<TMessage>() where TMessage : struct
			=> bus.UnsubscribeAll<TMessage>();
	}
}
