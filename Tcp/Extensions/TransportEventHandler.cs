using System;
using System.Threading;
using Sapientia.Extensions;
using Sapientia.Transport;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp.Extensions
{
	public class TransportEventHandler
	{
		private const int UPDATE_TIMEOUT = 3;

		public readonly TransportHandler_Tcp transportHandler;

		public event Action<ConnectionReference> OnConnectionReceived;
		public event Action<RemoteMessage> OnMessageReceived;

		public TransportEventHandler(TransportHandler_Tcp transportHandler)
		{
			this.transportHandler = transportHandler;
			OnConnectionReceived = default!;
			OnMessageReceived = default!;
		}

		public void Run()
		{
			while (transportHandler.GetState().HasNotIntFlag(TransportHandler_Tcp.State.Disposed))
			{
				if (transportHandler.connectionHandler.TryReceiveNewConnection(out var newConnection))
				{
					OnConnectionReceived?.Invoke(newConnection);
				}

				if (transportHandler.receivingHandler.TryBeginRead())
				{
					while (transportHandler.receivingHandler.TryRead(out var remoteMessage))
					{
						OnMessageReceived?.Invoke(remoteMessage);
					}
					transportHandler.receivingHandler.EndRead();
				}

				transportHandler.sendingHandler.TryApplySendStack();

				Thread.Sleep(UPDATE_TIMEOUT);
			}
		}
	}
}