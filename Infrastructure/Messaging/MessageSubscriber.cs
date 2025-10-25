using Messaging;

namespace Sapientia.Extensions
{
	public abstract class MessageSubscriber : CompositeDisposable
	{
		/// <summary>
		/// Подписывает на сообщение. Автоотписка при Dispose
		/// </summary>
		protected void Subscribe<TMessage>(Receiver<TMessage> receiver)
			where TMessage : struct =>
			AddDisposable(ControlledSubscribe(receiver));

		/// <summary>
		/// Подписывает на сообщение. Отсутствует автоотписка
		/// </summary>
		protected IMessageSubscriptionToken ControlledSubscribe<TMessage>(Receiver<TMessage> receiver)
			where TMessage : struct =>
			Messenger.Subscribe(receiver);

		/// <summary>
		/// Подписывает на сообщение. Автоотписка при Dispose
		/// </summary>
		protected void Subscribe<TContext, TMessage>(TContext context, Receiver<TMessage> receiver)
			where TMessage : struct =>
			AddDisposable(ControlledSubscribe(context, receiver));

		/// <summary>
		/// Подписывает на сообщение. Отсутствует автоотписка
		/// </summary>
		protected IMessageSubscriptionToken ControlledSubscribe<TContext, TMessage>(TContext context, Receiver<TMessage> receiver)
			where TMessage : struct =>
			Messenger<TContext>.Subscribe(context, receiver);
	}
}
