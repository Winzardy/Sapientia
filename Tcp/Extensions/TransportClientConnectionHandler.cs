using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Sapientia.Transport;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp.Extensions
{
	public class TransportClientConnectionHandler
	{
		public enum ConnectionState
		{
			WaitingForConnection,
			Connection,
			Connected,
			Disconnected,
		}

		public ConnectionReference ConnectionReference { get; private set; }

		public event Action<ConnectionReference> ConnectionReceivedEvent;
		public event Action ConnectionFailedEvent;
		public event Action ConnectionDeclinedEvent;

		public event Action ConnectionDisconnectedEvent;

		public event Action<RemoteMessage> MessageReceivedEvent;

		private int _customId;

		// Initialization context
		private ClientTransportService _clientTransportService;
		private IPEndPoint _endPoint;

		public ConnectionState State { get; private set; }

		public TransportClientConnectionHandler(ClientTransportService clientTransportService, string address, int port, int customId)
		{
			_clientTransportService = clientTransportService;
			_endPoint = new IPEndPoint(IPAddress.Parse(address), port);

			_customId = customId;

			SubscribeEvents();
			State = ConnectionState.WaitingForConnection;
		}

		private void SubscribeEvents()
		{
			_clientTransportService.ConnectionReceivedEvent += OnConnectionReceived;
			_clientTransportService.ConnectionFailedEvent += OnConnectionFailed;
			_clientTransportService.ConnectionDeclinedEvent += OnConnectionDeclined;

			_clientTransportService.ConnectionDisconnectedEvent += OnConnectionDisconnected;

			_clientTransportService.MessageReceivedEvent += OnMessageReceived;
		}

		public void Connect()
		{
			Debug.Assert(State != ConnectionState.Connected & State != ConnectionState.Connection);

			State = ConnectionState.Connection;
			_clientTransportService.Connect(_endPoint, _customId);
		}

		public async Task ConnectAsync()
		{
			State = ConnectionState.Connection;
			await _clientTransportService.ConnectAsync(_endPoint, _customId);
		}

		public void Disconnect()
		{
			_clientTransportService.Disconnect(ConnectionReference);
		}

		public void Send(RemoteMessageSender sender)
		{
			sender.SendAndExpand(ConnectionReference);
		}

		private void OnConnectionReceived(ConnectionReference connectionReference)
		{
			if (_customId != connectionReference.customId)
				return;

			ConnectionReference = connectionReference;

			State = ConnectionState.Connected;
			ConnectionReceivedEvent?.Invoke(connectionReference);
		}

		private void OnConnectionFailed(int customId)
		{
			if (_customId != customId)
				return;

			State = ConnectionState.Disconnected;
			ConnectionFailedEvent?.Invoke();
		}

		private void OnConnectionDeclined(Socket socket, int customId)
		{
			if (_customId != customId)
				return;

			State = ConnectionState.Disconnected;
			ConnectionDeclinedEvent?.Invoke();
		}

		private void OnConnectionDisconnected(int customId)
		{
			if (_customId != customId)
				return;

			State = ConnectionState.Disconnected;
			ConnectionDisconnectedEvent?.Invoke();
		}

		private void OnMessageReceived(RemoteMessage remoteMessage)
		{
			if (_customId != remoteMessage.connectionReference.customId)
				return;

			MessageReceivedEvent?.Invoke(remoteMessage);
		}
	}
}