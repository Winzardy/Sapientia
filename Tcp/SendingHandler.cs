using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp
{
	public class SendingHandler
	{
		private const int EXCHANGE_INTERVAL = 3;

		private class SendingBuffers : AsyncClass
		{
			public CircularBuffer<RemoteMessage> sendBuffer;
			public CircularBuffer<RemoteMessage> disposeBuffer;

			public SendingBuffers(int poolCapacity) : base(EXCHANGE_INTERVAL)
			{
				sendBuffer = new CircularBuffer<RemoteMessage>(poolCapacity);
				disposeBuffer = new CircularBuffer<RemoteMessage>(poolCapacity);
			}

			public void Dispose()
			{
				using (GetScope())
				{
					sendBuffer.Dispose();
					disposeBuffer.Dispose();
				}
			}
		}

		private readonly ConnectionHandler _connectionHandler;

		private readonly RemoteMessageStack _sendStack;
		private SendingBuffers _sendingBuffers;

		public SendingHandler(ConnectionHandler connectionHandler, int messageDataCapacity, int poolCapacity)
		{
			_connectionHandler = connectionHandler;

			_sendStack = new RemoteMessageStack(messageDataCapacity, poolCapacity, 2);
			_sendingBuffers = new SendingBuffers(poolCapacity);

			_sendStack.SetMillisecondsTimeout(EXCHANGE_INTERVAL);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RemoteMessageSender CreateMessageSender()
		{
			using (_sendStack.GetScope())
			{
				return _sendStack.GetSender();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryCreateMessageSender(out RemoteMessageSender messageSender)
		{
			if (_sendStack.TrySetBusy())
			{
				messageSender = default;
				return false;
			}

			messageSender = _sendStack.GetSender();

			_sendStack.SetFree();
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ApplySendStack()
		{
			using (_sendingBuffers.GetScope())
			{
				SendStack();
			}
		}

		public bool TryApplySendStack()
		{
			if (!_sendingBuffers.TrySetBusy())
				return false;
			SendStack();
			_sendingBuffers.SetFree();
			return true;
		}

		private void SendStack()
		{
			// Dispose handled messages
			while (!_sendingBuffers.disposeBuffer.IsEmpty)
			{
				_sendingBuffers.disposeBuffer.RemoveFirst().Dispose();
			}

			using (_sendStack.GetScope())
			{
				while (_sendStack.TryRead(out var remoteMessage))
				{
					_sendingBuffers.sendBuffer.AddLast(remoteMessage);
				}
			}
		}

		internal void Update()
		{
			SendMessages();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SendMessages()
		{
			if (!_sendingBuffers.TrySetBusy())
				return;

			while (!_sendingBuffers.sendBuffer.IsEmpty)
			{
				var remoteMessage = _sendingBuffers.sendBuffer.RemoveFirst();

				if (_connectionHandler.TryGetConnection(remoteMessage.connectionReference, out var connection))
				{
					connection.Send(remoteMessage);
				}

				_sendingBuffers.disposeBuffer.AddLast(remoteMessage);
			}

			_sendingBuffers.SetFree();
		}

		internal void Dispose()
		{
			_sendStack.Dispose();
			_sendingBuffers.Dispose();
		}
	}
}