namespace Sapientia.Tcp.Extensions
{
	public interface ITransportContext
	{
		public TransportHandler_Tcp CreateTransportHandler();
	}
}