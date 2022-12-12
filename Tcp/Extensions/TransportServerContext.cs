using System.Net;

namespace Sapientia.Tcp.Extensions
{
	public readonly struct TransportServerContext : ITransportContext
	{
		public readonly EndPoint endPoint;
		public readonly int maxConnections;
		public readonly int messageCapacity;
		public readonly int messageDataCapacity;

		public TransportServerContext(string address, int port, int maxConnections, int messageCapacity, int messageDataCapacity) :
			this(new IPEndPoint(IPAddress.Parse(address), port), maxConnections, messageCapacity, messageDataCapacity){}

		public TransportServerContext(EndPoint endPoint, int maxConnections, int messageCapacity, int messageDataCapacity)
		{
			this.endPoint = endPoint;
			this.maxConnections = maxConnections;
			this.messageCapacity = messageCapacity;
			this.messageDataCapacity = messageDataCapacity;
		}

		public TransportHandler_Tcp CreateTransportHandler()
		{
			var transportHandler = new TransportHandler_Tcp(maxConnections, messageCapacity, messageDataCapacity);
			transportHandler.SetupServer(endPoint);
			transportHandler.Start();

			return transportHandler;
		}
	}
}