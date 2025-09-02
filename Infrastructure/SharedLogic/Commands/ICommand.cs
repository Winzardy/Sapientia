using System;

namespace SharedLogic
{
	public interface ICommand
	{
		public bool Validate(ISharedRoot root, out Exception exception);
		public void Execute(ISharedRoot root);
	}

	public interface ITimedCommand : ICommand
	{
		public long Timestamp { get; }
		public void SetTimestamp(long timestamp);
	}
}
