using System;

namespace SharedLogic
{
	public abstract class TimedCommand : ITimedCommand
	{
		public long Timestamp { get; private set; }

		void ITimedCommand.SetTimestamp(long timestamp) => Timestamp = timestamp;

		public abstract bool Validate(ISharedRoot root, out Exception exception);

		public abstract void Execute(ISharedRoot root);
	}
}
