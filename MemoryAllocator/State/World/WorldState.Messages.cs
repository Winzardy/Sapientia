using Messaging;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct WorldState
	{
		public struct StartedMessage
		{
			public readonly WorldId worldId;

			public StartedMessage(WorldId worldId)
			{
				this.worldId = worldId;
			}
		}

		private void SendStartedMessage()
		{
			new StartedMessage(worldId).Send();
		}

		public struct BeginDisposeMessage
		{
			public readonly WorldId worldId;

			public BeginDisposeMessage(WorldId worldId)
			{
				this.worldId = worldId;
			}
		}

		private void SendBeginDisposeMessage()
		{
			new BeginDisposeMessage(worldId).Send();
		}

		public struct DisposedMessage
		{
			public readonly WorldId worldId;

			public DisposedMessage(WorldId worldId)
			{
				this.worldId = worldId;
			}
		}

		private void SendDisposedMessage()
		{
			new DisposedMessage(worldId).Send();
		}

	}
}
