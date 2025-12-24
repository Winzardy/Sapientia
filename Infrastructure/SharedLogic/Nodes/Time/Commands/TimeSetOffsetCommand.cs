using System;

namespace SharedLogic.Commands
{
	[Serializable]
	public struct TimeSetOffsetCommand : IDebugCommand
	{
		public TimeSpan offset;

		public TimeSetOffsetCommand(TimeSpan offset)
		{
			this.offset = offset;
		}

		public bool Validate(ISharedRoot root, out Exception exception)
		{
			exception = null;
			return true;
		}

		public void Execute(ISharedRoot root)
		{
			root.GetNode<TimeSharedNode>()
			   .SetTimeOffset(offset);
		}
	}
}
