using Messaging;

namespace Sapientia.MemoryAllocator
{
	public partial class World
	{
		public struct StartedMessage
		{
			public readonly World world;

			public StartedMessage(World world)
			{
				this.world = world;
			}
		}

		private void SendStartedMessage()
		{
			new StartedMessage(this).Send();
		}

		public struct LateUpdateMessage
		{
			public readonly World world;

			public LateUpdateMessage(World world)
			{
				this.world = world;
			}
		}

		private void SendLateUpdateMessage()
		{
			new LateUpdateMessage(this).Send();
		}

		public struct BeginDisposeMessage
		{
			public readonly World world;

			public BeginDisposeMessage(World world)
			{
				this.world = world;
			}
		}

		private void SendBeginDisposeMessage()
		{
			new BeginDisposeMessage(this).Send();
		}

		public struct DisposedMessage
		{
			public readonly World world;

			public DisposedMessage(World world)
			{
				this.world = world;
			}
		}

		private void SendDisposedMessage()
		{
			new DisposedMessage(this).Send();
		}
	}
}
