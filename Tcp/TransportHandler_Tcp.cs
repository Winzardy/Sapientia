using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Fusumity.Collections;
using Fusumity.Transport;
using Fusumity.Transport.RemoteMessage;
using Fusumity.Extensions;

namespace Fusumity.Tcp
{
	public class TransportHandler_Tcp
	{
		[Flags]
		public enum HandlerState
		{
			Initialized = 1 << 0,
			Started     = 1 << 1,
			Accepting   = 1 << 2,
			Connecting  = 1 << 3,
			Updating    = 1 << 4,
			Disposed    = 1 << 5,
		}

		public enum BufferState
		{
			Free = 0,
			Busy = 1,
		}

		public const int updateInterval = 10;

		public readonly int connectionsCapacity;
		public readonly int connectionMessagesCapacity;
		public readonly int messageBufferCapacity;

		internal Socket serverSocket;

		internal volatile int state;

		public readonly SendingHandler sendingHandler;
		public readonly ReceivingHandler receivingHandler;

		public HandlerState State
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => state.ToEnum<HandlerState>();
		}

		public TransportHandler_Tcp(int connectionsCapacity, int connectionMessagesCapacity, int messageBufferCapacity)
		{
			this.connectionsCapacity = connectionsCapacity;
			this.connectionMessagesCapacity = connectionMessagesCapacity;
			this.messageBufferCapacity = messageBufferCapacity;

			serverSocket = null!;

			var poolCapacity = connectionsCapacity * connectionMessagesCapacity;
			sendingHandler = new SendingHandler(this, messageBufferCapacity, poolCapacity);
			receivingHandler = new ReceivingHandler(this, messageBufferCapacity, poolCapacity);

			state = 0;

			Initialize();
		}

		private void Initialize()
		{

			state.AddIntFlag_Interlocked_ReturnInt(HandlerState.Initialized);
		}

		public void SetupClient()
		{
			Debug.Assert(state.HasIntFlag(HandlerState.Initialized) &
			             state.HasNotIntFlag(HandlerState.Disposed));

			StartConnecting();
		}

		public void SetupServer(EndPoint endPoint)
		{
			Debug.Assert(state.HasIntFlag(HandlerState.Initialized) &
			             state.HasNotIntFlag(HandlerState.Disposed));

			CreateServerSocket(endPoint);
			StartAccepting();
		}

		public void Start()
		{
			Debug.Assert(state.HasIntFlag(HandlerState.Initialized) &
			             state.HasNotIntFlag(HandlerState.Started) &
			             state.HasNotIntFlag(HandlerState.Disposed));

			StartUpdating();

			state.AddIntFlag_Interlocked_ReturnInt(HandlerState.Started);
		}

		private void CreateServerSocket(EndPoint endPoint)
		{
			serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			serverSocket.Bind(endPoint);
			serverSocket.Listen(connectionsCapacity);
		}

		private void StartAccepting()
		{
			state.AddIntFlag_Interlocked_ReturnInt(HandlerState.Accepting);

			Accepting();
		}

		private void EndAccepting()
		{
			state.Interlocked_RemoveIntFlag_ReturnInt(HandlerState.Accepting);
		}

		private void Accepting()
		{
			serverSocket.BeginAccept(connectionMessagesCapacity, OnAccepted, null);
		}

		private void OnAccepted(IAsyncResult asyncResult)
		{
			if (state.HasNotIntFlag(HandlerState.Accepting))
				return;

			var connectionSocket = serverSocket.EndAccept(out var acceptData, out var dataCount, asyncResult);
			if (IsConnectionValid(connectionSocket, acceptData, dataCount))
			{
				AcceptConnection(connectionSocket);
			}
			else
			{
				connectionSocket.Close();
			}

			Accepting();
		}

		private bool IsConnectionValid(Socket connectionSocket, byte[] acceptData, int dataCount)
		{
			return true; // Not Implemented
		}

		private void StartConnecting()
		{
			state.AddIntFlag_Interlocked_ReturnInt(HandlerState.Connecting);
		}

		public void Connect(EndPoint remoteEndPoint)
		{
			Debug.Assert(state.HasIntFlag(HandlerState.Started) & state.HasIntFlag(HandlerState.Connecting) &
			             state.HasNotIntFlag(HandlerState.Disposed));

			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.BeginConnect(remoteEndPoint, OnConnected, socket);
		}

		private void OnConnected(IAsyncResult asyncResult)
		{
			if (state.HasNotIntFlag(HandlerState.Connecting))
				return;

			var socket = (Socket)asyncResult.AsyncState!;
			var connectionData = GetConnectionData();

			var sendData = connectionData.Reader.Serialize();
			socket.Send(sendData);
			socket.EndConnect(asyncResult);

			connectionData.Dispose();

			AcceptConnection(socket);
		}

		private RemoteMessageSender GetConnectionData()
		{
			return sendingHandler.CreateMessageSender(); // Not Implemented
		}

		private void EndConnecting()
		{
			state.Interlocked_RemoveIntFlag_ReturnInt(HandlerState.Connecting);
		}

		private void StartUpdating()
		{
			state.AddIntFlag_Interlocked_ReturnInt(HandlerState.Updating);

			var updateThread = new Thread(Update);
			updateThread.Start();
		}

		private void EndUpdating()
		{
			state.Interlocked_RemoveIntFlag_ReturnInt(HandlerState.Updating);
		}

		private void Update()
		{
			while (state.HasIntFlag(HandlerState.Updating))
			{
				receivingHandler.Update();
				sendingHandler.Update();

				Thread.Sleep(updateInterval);
			}
		}

		public void Dispose()
		{
			EndAccepting();
			EndConnecting();
			EndUpdating();

			serverSocket?.Close();

			state.AddIntFlag_Interlocked_ReturnInt(HandlerState.Disposed);
		}

		public class ConnectionHandler
		{
			private readonly SparseSet<Connection_Tcp> _connections;
			private readonly SimpleList<Connection_Tcp> _existingConnections;

			public readonly ReadOnlySimpleList<Connection_Tcp> existingConnections;

			private volatile int _nextConnectionId;

			private CircularBuffer<ConnectionReference> _newConnectionBuffer;
			private volatile int _newConnectionsCount;

			private volatile int _state;

			public BufferState State
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _state.ToEnum<BufferState>();
			}

			public int NewConnectionsCount
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _newConnectionsCount;
			}

			internal ConnectionHandler(int connectionsCapacity, int connectionMessagesCapacity)
			{
				_connections = new SparseSet<Connection_Tcp>(connectionsCapacity);
				_existingConnections = new SimpleList<Connection_Tcp>(connectionsCapacity);
				existingConnections = new ReadOnlySimpleList<Connection_Tcp>(_existingConnections);
				_nextConnectionId = 0;

				_newConnectionBuffer = new CircularBuffer<ConnectionReference>(connectionsCapacity);
				_state = (int)BufferState.Free;

				var connections = _connections.GetValueArray();

				for (var i = 0; i < connections.Length; i++)
				{
					connections[i] = new Connection_Tcp(connectionMessagesCapacity);
				}
			}

			public void CloseConnection(ConnectionReference connectionReference)
			{
				if (TryGetConnection(connectionReference, out var connection))
					return;

				connection.Close();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool TryGetConnection(ConnectionReference connectionReference, out Connection_Tcp connection)
			{
				connection = _connections.GetValue(connectionReference.index);
				return connection.ConnectionReference != connectionReference;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal Connection_Tcp ForceGetConnection(int index)
			{
				return _connections.GetValue(index);
			}

			public bool TryReceiveNewConnection(out ConnectionReference connectionReference)
			{
				if (_newConnectionsCount > 0 && _state.Interlocked_CompareExchangeIntEnum(BufferState.Busy, BufferState.Free) == BufferState.Free)
				{
					while (!_newConnectionBuffer.IsEmpty)
					{
						connectionReference = _newConnectionBuffer.RemoveFirst();
						var connection = _connections.GetValue(connectionReference.index);

						if (connectionReference.id == connection.ConnectionReference.id)
						{
							Interlocked.Decrement(ref _newConnectionsCount);
							_state = (int)BufferState.Free;
							return true;
						}
					}

					_state = (int)BufferState.Free;
				}

				connectionReference = default;
				return false;
			}

			internal void Update()
			{
				if (_state.Interlocked_CompareExchangeIntEnum(BufferState.Busy, BufferState.Free) == BufferState.Busy)
					return;

				for (var i = 0; i < _existingConnections.Count; i++)
				{
					var connection = _existingConnections[i];

					if (connection.State == Connection_Tcp.ConnectionState.Disconnecting)
					{
						connection.Disconnect();
						_connections.ReleaseValueIndex(connection.ConnectionReference.index);
					}
					else
					{
						i++;
					}
				}
			}

			private void DisconnectClosedConnections()
			{

			}

			private void AcceptConnection(Socket connectionSocket)
			{
				while (_state.Interlocked_CompareExchangeIntEnum(BufferState.Busy, BufferState.Free) == BufferState.Busy)
				{
					Thread.Sleep(updateInterval);
				}

				if (_connections.IsFull)
				{
					// Overloaded
					connectionSocket.Close();
					return;
				}

				var index = _connections.AllocateValueIndex();
				var connection = _connections.GetValue(index);
				var connectionReference = new ConnectionReference(index, _nextConnectionId);

				connection.Start(connectionSocket, connectionReference);

				_newConnectionBuffer.AddLast(connectionReference);

				Interlocked.Increment(ref _nextConnectionId);
				Interlocked.Increment(ref _newConnectionsCount);
				_state = (int)BufferState.Free;
			}

			internal void Dispose()
			{
				for (var i = 0; i < _existingConnections.Count; i++)
				{
					_existingConnections[i].Disconnect();
				}
				_connections.Dispose();
			}
		}

		public class SendingHandler
		{
			private readonly ConnectionHandler _connectionHandler;

			private readonly RemoteMessageStack _sendStack;
			private CircularBuffer<RemoteMessage> _sendBuffer;
			private CircularBuffer<RemoteMessage> _disposeBuffer;

			private volatile int _state;

			public BufferState State
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _state.ToEnum<BufferState>();
			}

			public SendingHandler(ConnectionHandler connectionHandler, int messageCapacity, int poolCapacity)
			{
				_connectionHandler = connectionHandler;

				_sendStack = new RemoteMessageStack(messageCapacity, poolCapacity, 2);
				_sendBuffer = new CircularBuffer<RemoteMessage>(poolCapacity);
				_disposeBuffer = new CircularBuffer<RemoteMessage>(poolCapacity);

				_state = (int)BufferState.Free;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public RemoteMessageSender CreateMessageSender()
			{
				return _sendStack.GetSender();
			}

			public bool TryApplySendStack()
			{
				if (_state.Interlocked_CompareExchangeIntEnum(BufferState.Busy, BufferState.Free) == BufferState.Busy)
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

				_state = (int)BufferState.Free;
				return true;
			}

			internal void Update()
			{
				SendMessages();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void SendMessages()
			{
				if (_state.Interlocked_CompareExchangeIntEnum(BufferState.Busy, BufferState.Free) == BufferState.Busy)
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

				_state = (int)BufferState.Free;
			}
		}

		public class ReceivingHandler
		{
			private readonly ConnectionHandler _connectionHandler;

			private readonly RemoteMessageStack _receiveStack;
			private CircularBuffer<RemoteMessage> _receiveBuffer;
			private CircularBuffer<RemoteMessage> _disposeBuffer;

			private volatile int _state;
			private bool _isReading;

			public BufferState State
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _state.ToEnum<BufferState>();
			}

			public bool IsReading
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _isReading;
			}

			public ReceivingHandler(ConnectionHandler connectionHandler, int messageCapacity, int poolCapacity)
			{
				_connectionHandler = connectionHandler;

				_receiveStack = new RemoteMessageStack(messageCapacity, poolCapacity, 1, 0);
				_receiveBuffer = new CircularBuffer<RemoteMessage>(poolCapacity);
				_disposeBuffer = new CircularBuffer<RemoteMessage>(poolCapacity);

				_state = (int)BufferState.Free;
				_isReading = false;
			}

			public bool TryBeginRead()
			{
				if (_state.Interlocked_CompareExchangeIntEnum(BufferState.Busy, BufferState.Free) == BufferState.Free)
				{
					_isReading = true;
				}
				return false;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool TryRead(out RemoteMessage message)
			{
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

				_state = (int)BufferState.Free;
			}

			internal void Update()
			{
				ReceiveMessages();
				TryApplyReceiveStack();
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
				if (_state.Interlocked_CompareExchangeIntEnum(BufferState.Busy, BufferState.Free) == BufferState.Busy)
					return false;

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

				_state = (int)BufferState.Free;
				return true;
			}
		}
	}
}