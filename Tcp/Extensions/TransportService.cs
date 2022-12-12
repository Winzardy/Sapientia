using System;
using System.Net.Sockets;
using System.Threading;
using Sapientia.Extensions;
using Sapientia.Transport;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp.Extensions
{
	public abstract class TransportService : ITransportService
	{
		private const int UPDATE_TIMEOUT = 3;

		public readonly TransportHandler_Tcp transportHandler;

		public event Action<ConnectionReference> ConnectionReceivedEvent;
		public event Action<RemoteMessage> MessageReceivedEvent;
		public event Action<int> ConnectionFailedEvent
		{
			add => transportHandler.connectionHandler.ConnectionFailedEvent += value;
			remove => transportHandler.connectionHandler.ConnectionFailedEvent -= value;
		}
		public event Action<Socket, int> ConnectionDeclinedEvent
		{
			add => transportHandler.connectionHandler.ConnectionDeclinedEvent += value;
			remove => transportHandler.connectionHandler.ConnectionDeclinedEvent -= value;
		}

		public TransportService(in TransportHandler_Tcp transportHandler)
		{
			this.transportHandler = transportHandler;

			ConnectionReceivedEvent = default!;
			MessageReceivedEvent = default!;
		}

		public RemoteMessageSender GetMessageSender()
		{
			return transportHandler.sendingHandler.CreateMessageSender();
		}

		public void Run()
		{
			while (transportHandler.GetState().HasNotIntFlag(TransportHandler_Tcp.State.Disposed))
			{
				ManuallyUpdate();

				Thread.Sleep(UPDATE_TIMEOUT);
			}
		}

		public void ManuallyUpdate()
		{
			if (transportHandler.connectionHandler.TryReceiveNewConnection(out var newConnection))
			{
				ConnectionReceivedEvent?.Invoke(newConnection);
			}

			if (transportHandler.receivingHandler.TryBeginRead())
			{
				while (transportHandler.receivingHandler.TryRead(out var remoteMessage))
				{
					MessageReceivedEvent?.Invoke(remoteMessage);
				}
				transportHandler.receivingHandler.EndRead();
			}

			transportHandler.sendingHandler.TryApplySendStack();
		}
	}
}