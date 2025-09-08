using System;

namespace SharedLogic
{
	[Serializable]
	public struct TimedCommand : ICommand
	{
		private readonly long _timestamp;

		public TimedCommand(long timestamp)
		{
			_timestamp = timestamp;
		}

		public bool Validate(ISharedRoot root, out Exception exception)
		{
			var node = root.GetNode<TimeSharedNode>();

			exception = null;
			if (_timestamp < node.ServerTime.Ticks)
			{
				exception = new Exception("Invalid timestamp");
				return false;
			}

			return true;
		}

		public void Execute(ISharedRoot root)
		{
			root.GetNode<TimeSharedNode>()
			   .SetTimestamp(_timestamp);
		}
	}
}
