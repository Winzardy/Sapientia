using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Sapientia.Extensions;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp
{
	public class TransportHandler_Tcp
	{
		private const int UPDATE_INTERVAL = 10;

		[Flags]
		public enum State
		{
			Initialized = 1 << 0,
			Started     = 1 << 1,
			Accepting   = 1 << 2,
			Connecting  = 1 << 3,
			Updating    = 1 << 4,
			Disposed    = 1 << 5,
		}

		public readonly int connectionsCapacity;
		public readonly int messageCapacity;
		public readonly int messageDataCapacity;

		public readonly ConnectionHandler connectionHandler;
		public readonly SendingHandler sendingHandler;
		public readonly ReceivingHandler receivingHandler;

		private Socket _serverSocket;

		private volatile int _state;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public State GetState() => _state.ToEnum<State>();

		public TransportHandler_Tcp(int connectionsCapacity, int messageCapacity, int messageDataCapacity)
		{
			this.connectionsCapacity = connectionsCapacity;
			this.messageCapacity = messageCapacity;
			this.messageDataCapacity = messageDataCapacity;

			_serverSocket = null!;

			var poolCapacity = connectionsCapacity * messageCapacity;

			connectionHandler = new ConnectionHandler(connectionsCapacity, messageCapacity);
			sendingHandler = new SendingHandler(connectionHandler, messageDataCapacity, poolCapacity);
			receivingHandler = new ReceivingHandler(connectionHandler, messageDataCapacity, poolCapacity);

			_state = (int)State.Initialized;

			Initialize();
		}

		private void Initialize()
		{
			_state.AddIntFlag_Interlocked_ReturnInt(State.Initialized);
		}

		public void SetupClient()
		{
			Debug.Assert(_state.HasIntFlag(State.Initialized) &
			             _state.HasNotIntFlag(State.Disposed));

			StartConnecting();
		}

		public void SetupServer(string address, int port)
		{
			SetupServer(new IPEndPoint(IPAddress.Parse(address), port));
		}

		public void SetupServer(EndPoint endPoint)
		{
			Debug.Assert(_state.HasIntFlag(State.Initialized) &
			             _state.HasNotIntFlag(State.Disposed));

			CreateServerSocket(endPoint);
			StartAccepting();
		}

		public void Start()
		{
			Debug.Assert(_state.HasIntFlag(State.Initialized) &
			             _state.HasNotIntFlag(State.Started) &
			             _state.HasNotIntFlag(State.Disposed));

			StartUpdating();

			_state.AddIntFlag_Interlocked_ReturnInt(State.Started);
		}

		private void CreateServerSocket(EndPoint endPoint)
		{
			_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_serverSocket.Bind(endPoint);
			_serverSocket.Listen(connectionsCapacity);
		}

		private void StartAccepting()
		{
			_state.AddIntFlag_Interlocked_ReturnInt(State.Accepting);

			Accepting();
		}

		private void EndAccepting()
		{
			_state.Interlocked_RemoveIntFlag_ReturnInt(State.Accepting);
		}

		private void Accepting()
		{
			_serverSocket.BeginAccept(messageDataCapacity, OnAccepted, null);
		}

		private void OnAccepted(IAsyncResult asyncResult)
		{
			if (_state.HasNotIntFlag(State.Accepting))
				return;

			var connectionSocket = _serverSocket.EndAccept(out var acceptData, out var dataCount, asyncResult);
			if (IsConnectionValid(connectionSocket, acceptData, dataCount))
			{
				connectionHandler.AcceptConnection(connectionSocket, -1);
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
			_state.AddIntFlag_Interlocked_ReturnInt(State.Connecting);
		}

		public void Connect(EndPoint remoteEndPoint, int customId = -1)
		{
			Debug.Assert(_state.HasIntFlag(State.Started) & _state.HasIntFlag(State.Connecting) &
			             _state.HasNotIntFlag(State.Disposed));

			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.BeginConnect(remoteEndPoint, OnConnected, (socket, customId));
		}

		private void OnConnected(IAsyncResult asyncResult)
		{
			if (_state.HasNotIntFlag(State.Connecting))
				return;

			var result = ((Socket socket, int customId))asyncResult.AsyncState!;

			var socket = result.socket;
			var customId = result.customId;
			if (!socket.Connected)
			{
				connectionHandler.AcceptConnection(null!, customId);
				socket.Dispose();
				return;
			}

			var connectionData = GetConnectionData();
			var sendData = connectionData.Reader.Serialize();

			socket.Send(sendData);
			socket.EndConnect(asyncResult);

			connectionData.Dispose();

			connectionHandler.AcceptConnection(socket, customId);
		}

		private RemoteMessageSender GetConnectionData()
		{
			return sendingHandler.CreateMessageSender(); // Not Implemented
		}

		private void EndConnecting()
		{
			_state.Interlocked_RemoveIntFlag_ReturnInt(State.Connecting);
		}

		private void StartUpdating()
		{
			_state.AddIntFlag_Interlocked_ReturnInt(State.Updating);

			var updateThread = new Thread(Update);
			updateThread.Start();
		}

		private void EndUpdating()
		{
			_state.Interlocked_RemoveIntFlag_ReturnInt(State.Updating);
		}

		private void Update()
		{
			while (_state.HasIntFlag(State.Updating))
			{
				connectionHandler.Update();
				receivingHandler.Update();
				sendingHandler.Update();

				Thread.Sleep(UPDATE_INTERVAL);
			}
		}

		public void Dispose()
		{
			EndAccepting();
			EndConnecting();
			EndUpdating();

			connectionHandler.Dispose();
			receivingHandler.Dispose();
			sendingHandler.Dispose();

			_serverSocket?.Close();

			_state.AddIntFlag_Interlocked_ReturnInt(State.Disposed);
		}
	}
}