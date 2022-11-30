using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Sapientia.Collections;
using Sapientia.Extensions;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp
{
	public class ReceivingHandler
	{
		public enum State
		{
			Free = 0,
			Busy = 1,
		}

		private const int EXCHANGE_INTERVAL = 10;

		private readonly ConnectionHandler _connectionHandler;

		private readonly RemoteMessageStack _receiveStack;
		private CircularBuffer<RemoteMessage> _receiveBuffer;
		private CircularBuffer<RemoteMessage> _disposeBuffer;

		private volatile int _state;
		private bool _isReading;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public State GetState() => _state.ToEnum<State>();

		public bool IsReading
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _isReading;
		}

		public ReceivingHandler(ConnectionHandler connectionHandler, int messageCapacity,
			int poolCapacity)
		{
			_connectionHandler = connectionHandler;

			_receiveStack = new RemoteMessageStack(messageCapacity, poolCapacity, 1, 0);
			_receiveBuffer = new CircularBuffer<RemoteMessage>(poolCapacity);
			_disposeBuffer = new CircularBuffer<RemoteMessage>(poolCapacity);

			_state = (int)State.Free;
			_isReading = false;
		}

		public void BeginRead()
		{
			while (!TryBeginRead())
			{
				Thread.Sleep(EXCHANGE_INTERVAL);
			}
		}

		public bool TryBeginRead()
		{
			if (_state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Free)
			{
				if (_isReading)
				{
					throw new Exception("TryBeginRead out of state!");
				}
				_isReading = true;
				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryRead(out RemoteMessage message)
		{
			if (!_isReading)
			{
				throw new Exception("TryRead out of state!");
			}

			if (_receiveBuffer.IsEmpty)
			{
				message = default!;
				return false;
			}

			message = _receiveBuffer.RemoveLast();
			_disposeBuffer.AddLast(message);

			return true;
		}

		public void EndRead()
		{
			if (!_isReading)
			{
				throw new Exception("Reading was ended out of state!");
			}

			_isReading = false;
			_state = (int)State.Free;
		}

		internal void Update()
		{
			if (_state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Busy)
				return;

			ReceiveMessages();
			TryApplyReceiveStack();

			_state = (int)State.Free;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ReceiveMessages()
		{
			var connections = _connectionHandler.existingConnections;

			for (var i = 0; i < connections.Count; i++)
			{
				connections[i].Receive(_receiveStack);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool TryApplyReceiveStack()
		{
			// Dispose handled messages
			while (!_disposeBuffer.IsEmpty)
			{
				var message = _disposeBuffer.RemoveFirst();

				var connection = _connectionHandler.ForceGetConnection(message.connectionReference.index);
				connection.OnReadMessage();

				message.Dispose();
			}

			while (_receiveStack.TryRead(out var remoteMessage))
			{
				_receiveBuffer.AddLast(remoteMessage);
			}

			return true;
		}

		internal void Dispose()
		{
			_connectionHandler.Dispose();
			_receiveStack.Dispose();
			_receiveBuffer.Dispose();
			_disposeBuffer.Dispose();
		}
	}
}