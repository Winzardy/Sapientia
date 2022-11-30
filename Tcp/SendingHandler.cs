using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Extensions;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp
{
	public class SendingHandler
	{
		private const int EXCHANGE_INTERVAL = 10;
		public enum State
		{
			Free = 0,
			Busy = 1,
		}

		private readonly ConnectionHandler _connectionHandler;

		private readonly RemoteMessageStack _sendStack;
		private CircularBuffer<RemoteMessage> _sendBuffer;
		private CircularBuffer<RemoteMessage> _disposeBuffer;

		private volatile int _state;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public State GetState() => _state.ToEnum<State>();

		public SendingHandler(ConnectionHandler connectionHandler, int messageDataCapacity,
			int poolCapacity)
		{
			_connectionHandler = connectionHandler;

			_sendStack = new RemoteMessageStack(messageDataCapacity, poolCapacity, 2);
			_sendBuffer = new CircularBuffer<RemoteMessage>(poolCapacity);
			_disposeBuffer = new CircularBuffer<RemoteMessage>(poolCapacity);

			_state = (int)State.Free;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RemoteMessageSender CreateMessageSender()
		{
			RemoteMessageSender result;
			while (!TryCreateMessageSender(out result))
			{
				Thread.Sleep(EXCHANGE_INTERVAL);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryCreateMessageSender(out RemoteMessageSender messageSender)
		{
			if (_state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Busy)
			{
				messageSender = default;
				return false;
			}

			messageSender = _sendStack.GetSender();
			_state = (int)State.Free;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ApplySendStack()
		{
			while (!TryApplySendStack())
			{
				Thread.Sleep(EXCHANGE_INTERVAL);
			}
		}

		public bool TryApplySendStack()
		{
			if (_state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Busy)
				return false;

			// Dispose handled messages
			while (!_disposeBuffer.IsEmpty)
			{
				_disposeBuffer.RemoveFirst().Dispose();
			}

			while (_sendStack.TryRead(out var remoteMessage))
			{
				_sendBuffer.AddLast(remoteMessage);
			}

			_state = (int)State.Free;
			return true;
		}

		internal void Update()
		{
			SendMessages();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SendMessages()
		{
			if (_state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Busy)
				return;

			while (!_sendBuffer.IsEmpty)
			{
				var remoteMessage = _sendBuffer.RemoveFirst();

				if (_connectionHandler.TryGetConnection(remoteMessage.connectionReference, out var connection))
				{
					connection.Send(remoteMessage);
				}

				_disposeBuffer.AddLast(remoteMessage);
			}

			_state = (int)State.Free;
		}

		internal void Dispose()
		{
			_sendStack.Dispose();
			_sendBuffer.Dispose();
			_disposeBuffer.Dispose();
		}
	}
}