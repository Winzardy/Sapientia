using System;
using Sapientia;

namespace SharedLogic
{
	/// <summary>
	/// Системная команда, которая отправляется перед каждой командой, чтобы двигать время
	/// </summary>
	[Serializable]
	public struct TimeSetCommand : ICommand
	{
		public long timestamp;

		public TimeSetCommand(long timestamp)
		{
			this.timestamp = timestamp;
		}

		public bool Validate(ISharedRoot root, out Exception exception)
		{
			var node = root.GetNode<TimeSharedNode>();

			if (node.CanSetTimestamp(timestamp, out var error))
			{
				if (error.TryGetValue(out var errorValue))
					exception = new Exception(errorValue.ToString());
				else
					exception = this.GetDefaultException();
				return false;
			}

			exception = null;
			return true;
		}

		public void Execute(ISharedRoot root)
		{
			root.GetNode<TimeSharedNode>()
			   .SetTimestamp(timestamp);
		}
	}
}
