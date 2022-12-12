using System.Net;
using System.Threading.Tasks;

namespace Sapientia.Tcp.Extensions
{
	public class ClientTransportService : TransportService, IClientTransportService
	{
		public ClientTransportService(in TransportClientContext serverContext) : base(serverContext.CreateTransportHandler()) {}

		public void Connect(EndPoint remoteEndPoint, int customId = -1)
		{
			transportHandler.Connect(remoteEndPoint, customId);
		}

		public async Task ConnectAsync(EndPoint remoteEndPoint, int customId = -1)
		{
			await transportHandler.ConnectAsync(remoteEndPoint, customId);
		}
	}
}