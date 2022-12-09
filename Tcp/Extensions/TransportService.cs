using System;
using System.Threading;
using Sapientia.Extensions;
using Sapientia.Transport;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp.Extensions
{
	public class TransportService : ITransportService
	{
		private const int UPDATE_TIMEOUT = 3;

		public readonly TransportHandler_Tcp transportHandler;

		public event Action<ConnectionReference> ConnectionReceivedEvent;
		public event Action<RemoteMessage> MessageReceivedEvent;

		public TransportService(in TransportServerContext serverContext)
		{
			transportHandler = serverContext.CreateTransportHandler();

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