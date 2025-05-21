using System.Runtime.CompilerServices;
using Sapientia;
using Sapientia.Data;

namespace Messaging
{
	public delegate bool Filter<T>(in T msg);

	/// <summary>
	/// Централизованный мессендж-хаб, отвечающий за подписку, публикацию и доставку сообщений.
	/// </summary>
	public sealed partial class MessageBus : AsyncClass
	{
		/// <summary>
		/// Подписка на тип сообщения с заданным обработчиком.
		/// Все ссылки хранятся как сильные (strong references).
		///
		/// Все сообщения этого типа будут доставляться.
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <param name="receiver">Метод, вызываемый при доставке сообщения</param>
		/// <returns>Токен подписки, необходимый для отписки</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Receiver<TMessage> receiver) where TMessage : struct
			=> AddSubscriptionInternal(receiver, null, true);

		/// <summary>
		/// Подписка на тип сообщения с заданным обработчиком.
		/// Все сообщения этого типа будут доставляться
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <param name="receiver">Метод, вызываемый при доставке сообщения</param>
		/// <param name="useStrongReferences">Использовать ли сильные ссылки на обработчик</param>
		/// <returns>Токен подписки, необходимый для отписки</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Receiver<TMessage> receiver, bool useStrongReferences)
			where TMessage : struct
			=> AddSubscriptionInternal(receiver, null, useStrongReferences);

		/// <summary>
		/// Подписка на тип сообщения с обработчиком и фильтром.
		/// Все ссылки (кроме прокси) хранятся как слабые (WeakReference).
		///
		/// Будут доставлены только те сообщения, которые проходят фильтр
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <param name="receiver">Метод, вызываемый при доставке сообщения</param>
		/// <param name="filter">Фильтр, определяющий, следует ли доставлять сообщение</param>
		/// <returns>Токен подписки, необходимый для отписки</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(Receiver<TMessage> receiver, Filter<TMessage> filter)
			where TMessage : struct
			=> AddSubscriptionInternal(receiver, filter, true);

		/// <summary>
		/// Подписка на тип сообщения с обработчиком и фильтром.
		/// Все ссылки хранятся как слабые (WeakReference)
		///
		/// Будут доставлены только те сообщения, которые проходят фильтр
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <param name="receiver">Метод, вызываемый при доставке сообщения</param>
		/// <param name="filter">Фильтр, определяющий, следует ли доставлять сообщение</param>
		/// <param name="useStrongReferences">Использовать ли сильные ссылки</param>
		/// <returns>Токен подписки, необходимый для отписки</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MessageSubscriptionToken<TMessage> Subscribe<TMessage>(
			Receiver<TMessage> receiver,
			Filter<TMessage> filter,
			bool useStrongReferences)
			where TMessage : struct
			=> AddSubscriptionInternal(receiver, filter, useStrongReferences);

		/// <summary>
		/// Отписка от конкретного типа сообщения.
		///
		/// Не выбрасывает исключение, если подписка не найдена.
		/// </summary>
		/// <param name="subscriptionToken">Токен подписки, полученный при подписке</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Unsubscribe<TMessage>(MessageSubscriptionToken<TMessage> subscriptionToken) where TMessage : struct
			=> RemoveSubscriptionInternal(subscriptionToken);

		/// <summary>
		/// Удаляет всех подписчиков на конкретный тип сообщения.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UnsubscribeAll<TMessage>() where TMessage : struct
			=> RemoveAllSubscriptionInternal<TMessage>();

		/// <summary>
		/// Публикует сообщение всем подписчикам.
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <param name="msg">Сообщение для доставки</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Send<TMessage>(ref TMessage msg) where TMessage : struct => SendInternal(ref msg);

		/// <summary>
		/// Публикует сообщение всем подписчикам и затем удаляет всех подписчиков этого типа.
		/// </summary>
		/// <typeparam name="TMessage">Тип сообщения</typeparam>
		/// <param name="msg">Сообщение для доставки</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SendAndUnsubscribeAll<TMessage>(ref TMessage msg) where TMessage : struct
			=> SendAndUnsubscribeAllInternal(ref msg);


	}
}
