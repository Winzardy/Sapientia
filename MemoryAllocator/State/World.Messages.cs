using Sapientia.Messaging;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct WorldState
	{
		public struct StartedMessage
		{
			public readonly AllocatorId allocatorId;

			public StartedMessage(AllocatorId allocatorId)
			{
				this.allocatorId = allocatorId;
			}
		}

		private void SendStartedMessage()
		{
			new StartedMessage(allocatorId).Send();
		}

		public struct BeginDisposeMessage
		{
			public readonly AllocatorId allocatorId;

			public BeginDisposeMessage(AllocatorId allocatorId)
			{
				this.allocatorId = allocatorId;
			}
		}

		private void SendBeginDisposeMessage()
		{
			new BeginDisposeMessage(allocatorId).Send();
		}

		public struct DisposedMessage
		{
			public readonly AllocatorId allocatorId;

			public DisposedMessage(AllocatorId allocatorId)
			{
				this.allocatorId = allocatorId;
			}
		}

		private void SendDisposedMessage()
		{
			new DisposedMessage(allocatorId).Send();
		}

	}
}
