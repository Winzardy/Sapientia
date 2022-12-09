namespace Sapientia.Tcp.Extensions
{
	public readonly struct TransportClientContext
	{
		public readonly int maxConnections;
		public readonly int messageCapacity;
		public readonly int messageDataCapacity;

		public TransportClientContext(int maxConnections, int messageCapacity, int messageDataCapacity)
		{
			this.maxConnections = maxConnections;
			this.messageCapacity = messageCapacity;
			this.messageDataCapacity = messageDataCapacity;
		}

		public TransportHandler_Tcp CreateTransportHandler()
		{
			var transportHandler = new TransportHandler_Tcp(maxConnections, messageCapacity, messageDataCapacity);
			transportHandler.SetupClient();
			transportHandler.Start();

			return transportHandler;
		}
	}
}