using System.Runtime.CompilerServices;
using Fusumity.Collections;
using Fusumity.Collections.ByteReader;

namespace Fusumity.Transport.RemoteMessage
{
	public class RemoteMessageStack
	{
		public readonly int messageCapacity;

		private ByteReaderPool _readerPool;

		private CircularBuffer<RemoteMessage> _buffer;

		public int BufferCapacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _buffer.Capacity;
		}

		public int PoolCapacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _readerPool.Capacity;
		}

		public int PoolAllocatedCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _readerPool.AllocatedCount;
		}

		internal RemoteMessageStack(int messageCapacity, int poolCapacity, int poolCount) : this(messageCapacity, poolCapacity, poolCount, poolCount) {}

		internal RemoteMessageStack(int messageCapacity, int poolCapacity, int poolCount, int poolCountExpandStep)
		{
			this.messageCapacity = messageCapacity;

			_readerPool = new ByteReaderPool(messageCapacity, poolCapacity, poolCount, poolCountExpandStep);
			_buffer = new CircularBuffer<RemoteMessage>(poolCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryRead(out RemoteMessage remoteMessage)
		{
			if (_buffer.IsEmpty)
			{
				remoteMessage = default;
				return false;
			}

			remoteMessage = _buffer.RemoveFirst();
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RemoteMessageSender GetSender()
		{
			return new RemoteMessageSender(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void AddMessage(RemoteMessage remoteMessage)
		{
			_buffer.AddLast(remoteMessage);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void AddMessageAndExpand(RemoteMessage remoteMessage)
		{
			_buffer.AddLastAndExpand(remoteMessage);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ByteReaderPool.Element AllocateReader()
		{
			return _readerPool.AllocateWithExpand();
		}
	}
}