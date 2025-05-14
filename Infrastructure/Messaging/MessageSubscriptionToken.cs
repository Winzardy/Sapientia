using System;

namespace Sapientia.Messaging
{
	public interface IMessageSubscriptionToken : IDisposable
	{
		public int HubIndex { get; }
		public void Unsubscribe() => Dispose();
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
			MessengerHub.SubscriptionMap<T>.Remove(this);
			GC.SuppressFinalize(this);
		}
	}
}
