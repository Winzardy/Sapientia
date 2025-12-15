using System;

namespace SharedLogic
{
	public enum CommandPriority
	{
		Passive,
		/// <summary>
		/// Наличие команды с этим приоритетом приводит к немедленной попытке отправить все команды
		/// </summary>
		Immediately,
	}

	public interface ICommand
	{
		public CommandPriority Priority => CommandPriority.Passive;

		public bool Validate(ISharedRoot root, out Exception exception);
		public void Execute(ISharedRoot root);
	}

	// TODO: необходимо проверять user permission на исполнение таких команд
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
			return new Exception($"Validation failed! Command type [ {typeof(T)} ] ({command})");
		}
	}
}
