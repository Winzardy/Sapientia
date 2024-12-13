using Sapientia.Messaging;

namespace Sapientia.Extensions
{
	public abstract class MessageSubscriber : CompositeDisposable
	{
		public override void Dispose()
		{
			OnDispose();

			base.Dispose();
		}

		protected virtual void OnDispose()
		{
		}

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
	}
}
