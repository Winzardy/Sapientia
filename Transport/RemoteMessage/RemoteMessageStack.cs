using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Collections.ByteReader;
using Sapientia.Data;

namespace Sapientia.Transport.RemoteMessage
{
	public class RemoteMessageStack : AsyncClass
	{
		public readonly int messageDataCapacity;

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

		internal RemoteMessageStack(int messageDataCapacity, int poolCapacity, int poolCount) : this(messageDataCapacity,
			poolCapacity, poolCount, poolCount)
		{
		}

		internal RemoteMessageStack(int messageDataCapacity, int poolCapacity, int poolCount, int poolCountExpandStep)
		{
			this.messageDataCapacity = messageDataCapacity;

			_readerPool = new ByteReaderPool(messageDataCapacity, poolCapacity, poolCount, poolCountExpandStep);
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
		internal void AddMessage_Interlocked(RemoteMessage remoteMessage)
		{
			using (GetScope())
			{
				AddMessage(remoteMessage);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void AddMessageAndExpand(RemoteMessage remoteMessage)
		{
			_buffer.AddLastAndExpand(remoteMessage);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void AddMessageAndExpand_Interlocked(RemoteMessage remoteMessage)
		{
			using (GetScope())
			{
				AddMessageAndExpand(remoteMessage);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ByteReaderPool.Element AllocateReader_Interlocked()
		{
			return _readerPool.AllocateWithExpand_Interlocked();
		}

		public void Dispose()
		{
			_readerPool.Dispose();
			_buffer.Dispose();
		}
	}
}