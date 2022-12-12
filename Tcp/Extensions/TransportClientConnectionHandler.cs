using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Sapientia.Transport;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp.Extensions
{
	public class TransportClientConnectionHandler
	{
		public ConnectionReference Connection { get; private set; }

		public event Action<ConnectionReference> ConnectionReceivedEvent;
		public event Action ConnectionFailedEvent;
		public event Action ConnectionDeclinedEvent;
		public event Action<RemoteMessage> MessageReceivedEvent;

		private int _customId;

		// Initialization context
		private ClientTransportService _clientTransportService;
		private IPEndPoint _endPoint;

		public bool IsConnected => Connection != default;

		public TransportClientConnectionHandler(ClientTransportService clientTransportService, string address, int port, int customId)
		{
			_clientTransportService = clientTransportService;
			_endPoint = new IPEndPoint(IPAddress.Parse(address), port);

			_customId = customId;
		}

		private void SubscribeEvents()
		{
			_clientTransportService.ConnectionReceivedEvent += OnConnectionReceived;
			_clientTransportService.ConnectionFailedEvent += OnConnectionFailed;
			_clientTransportService.ConnectionDeclinedEvent += OnConnectionDeclined;
			_clientTransportService.MessageReceivedEvent += OnMessageReceived;
		}

		public void Connect()
		{
			SubscribeEvents();
			_clientTransportService.Connect(_endPoint, _customId);
		}

		public async Task ConnectAsync()
		{
			SubscribeEvents();
			await _clientTransportService.ConnectAsync(_endPoint, _customId);
		}

		private void OnConnectionReceived(ConnectionReference connectionReference)
		{
			if (_customId != connectionReference.customId)
				return;

			Connection = connectionReference;
			ConnectionReceivedEvent?.Invoke(connectionReference);
		}

		private void OnConnectionFailed(int customId)
		{
			if (_customId != customId)
				return;

			ConnectionFailedEvent?.Invoke();
		}

		private void OnConnectionDeclined(Socket socket, int customId)
		{
			if (_customId != customId)
				return;

			ConnectionDeclinedEvent?.Invoke();
		}

		private void OnMessageReceived(RemoteMessage remoteMessage)
		{
			if (_customId != remoteMessage.connectionReference.customId)
				return;

			MessageReceivedEvent?.Invoke(remoteMessage);
		}
	}
}