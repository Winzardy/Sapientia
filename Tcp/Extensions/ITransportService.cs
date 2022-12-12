using System;
using System.Net.Sockets;
using Sapientia.Extensions;
using Sapientia.Transport;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp.Extensions
{
	public interface ITransportService : IService
	{
		public event Action<ConnectionReference> ConnectionReceivedEvent;
		public event Action<RemoteMessage> MessageReceivedEvent;
		public event Action<int> ConnectionFailedEvent;
		public event Action<Socket, int> ConnectionDeclinedEvent;

		public RemoteMessageSender GetMessageSender();

		public void Run();

		public void ManuallyUpdate();
	}
}