namespace Sapientia.Tcp.Extensions
{
	public class ServerTransportService : TransportService
	{
		public ServerTransportService(in TransportServerContext serverContext) : base(serverContext.CreateTransportHandler()) {}
	}
}