using Sapientia.Messaging;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public partial struct  World
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

		/*public struct LateUpdateMessage
		{
			public readonly AllocatorId allocatorId;

			public LateUpdateMessage(AllocatorId allocatorId)
			{
				this.allocatorId = allocatorId;
			}
		}

		private void SendLateUpdateMessage()
		{
			new LateUpdateMessage(allocatorId).Send();
		}*/

		/*public struct LateUpdateOnceMessage
		{
			public readonly AllocatorId allocatorId;

			public LateUpdateOnceMessage(AllocatorId allocatorId)
			{
				this.allocatorId = allocatorId;
			}
		}

		private void SendLateUpdateOnceMessage()
		{
			new LateUpdateOnceMessage(allocatorId).SendAndUnsubscribeAll();
		}*/

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
