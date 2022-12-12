using System.Net;

namespace Sapientia.Tcp.Extensions
{
	public interface IClientTransportService : ITransportService
	{
		public void Connect(EndPoint remoteEndPoint, int customId = -1);
	}
}