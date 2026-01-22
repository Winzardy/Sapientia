using System;
using Sapientia;

namespace Messaging
{
	public interface IMessageSubscriptionToken : IDisposable
	{
		public int HubIndex { get; }
		public void Unsubscribe() => Dispose();
	}

	public struct MessageSubscriptionToken : IDisposable
	{
		private IMessageSubscriptionToken _token;

		public int HubIndex => _token.HubIndex;

		public bool TrySubscribe<TMessage>(Receiver<TMessage> receiver)
			where TMessage : struct
		{
			var result = _token == null;
			if (result)
				_token = Messenger.Subscribe<TMessage>(receiver);
			return result;
		}

		public void Subscribe<TMessage>(Receiver<TMessage> receiver)
			where TMessage : struct
		{
			E.ASSERT(_token == null);
			_token = Messenger.Subscribe<TMessage>(receiver);
		}

		public void Dispose()
		{
			_token?.Unsubscribe();
			_token = null;
		}
	}

	/// <summary>
	/// Представляет активную подписку на сообщение.
	/// </summary>
	public sealed class MessageSubscriptionToken<T> : IMessageSubscriptionToken
		where T : struct
	{
		private readonly int _hubIndex;

		public int HubIndex => _hubIndex;

		/// <summary>
		/// Инициализирует новый экземпляр класса MessageSubscriptionToken.
		/// </summary>
		/// <param name="hub">Экземпляр MessengerHub, в который зарегистрирована подписка</param>
		internal MessageSubscriptionToken(int hubIndex)
		{
			_hubIndex = hubIndex;
		}

		public void Dispose()
		{
			MessageBus.SubscriptionMap<T>.Remove(this);
			GC.SuppressFinalize(this);
		}
	}
}
