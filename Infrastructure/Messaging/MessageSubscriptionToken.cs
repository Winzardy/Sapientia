using System;

namespace Sapientia.Messaging
{
	public interface IMessageSubscriptionToken : IDisposable
	{
		public void Unsubscribe() => Dispose();
	}

	/// <summary>
	/// Represents an active subscription to a message
	/// </summary>
	public sealed class MessageSubscriptionToken<T> : IMessageSubscriptionToken where T : struct
	{
		private readonly WeakReference<MessengerHub> _hub;

		/// <summary>
		/// Initializes a new instance of the MessageSubscriptionToken class.
		/// </summary>
		internal MessageSubscriptionToken(MessengerHub hub)
		{
			if (hub == null)
				throw new ArgumentNullException(nameof(hub));

			_hub = new WeakReference<MessengerHub>(hub);
		}

		public void Dispose()
		{
			if (_hub != null && _hub.TryGetTarget(out var hub))
				hub.Unsubscribe(this);

			GC.SuppressFinalize(this);
		}
	}
}
