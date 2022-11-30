using System.Runtime.CompilerServices;
using Sapientia.Collections.ByteReader;

namespace Sapientia.Transport.RemoteMessage
{
	public struct RemoteMessage : IDisposable
	{
		public readonly ConnectionReference connectionReference;

		private ByteReaderPool.Element _readerContainer;

		public readonly ByteReader Reader
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _readerContainer.Reader;
		}

		public RemoteMessage(ConnectionReference connectionReference, ByteReaderPool.Element readerContainer)
		{
			this.connectionReference = connectionReference;
			_readerContainer = readerContainer;
		}

		public void Dispose()
		{
			_readerContainer.Dispose();
		}
	}
}