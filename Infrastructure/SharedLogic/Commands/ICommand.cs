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
	}

	/// <summary>
	/// Только в Debug билде
	/// </summary>
	public interface IDebugCommand : ICommand
	{
	}

	public static class CommandExceptionUtility
	{
		public static Exception GetDefaultException<T>(this T command)
			where T : struct, ICommand
		{
			return new Exception($"Invalid command by type [ {typeof(T)} ] ({command})");
		}
	}
}
