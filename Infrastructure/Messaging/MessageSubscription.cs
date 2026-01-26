using System;
using JetBrains.Annotations;
using Messaging;
using Sapientia;

namespace Submodules.Sapientia.Infrastructure.Messaging
{
	[MustDisposeResource]
	public struct MessageSubscription<TMessage> : IDisposable
		where TMessage : struct
	{
		private MessageSubscription _token;

		public int HubIndex => _token.HubIndex;

		public bool TrySubscribe(Receiver<TMessage> receiver)
		{
			return _token.TrySubscribe(receiver);
		}

		public void Subscribe(Receiver<TMessage> receiver)
		{
			_token.Subscribe(receiver);
		}

		public void Dispose()
		{
			_token.Dispose();
		}
	}

	[MustDisposeResource]
	public struct MessageSubscription : IDisposable
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
}
