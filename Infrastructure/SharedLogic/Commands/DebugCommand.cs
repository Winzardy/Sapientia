using System;

namespace SharedLogic
{
	/// <summary>
	/// Только в Debug билде
	/// </summary>
	public abstract class DebugCommand : TimedCommand
	{
		public override bool Validate(ISharedRoot root, out Exception exception)
		{
			exception = null;
			return true;
		}
	}
}
