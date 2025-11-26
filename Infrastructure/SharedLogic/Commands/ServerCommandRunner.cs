using Sapientia;

namespace SharedLogic
{
	/// <summary>
	/// Раннер на сервере
	/// </summary>
	public readonly struct ServerCommandRunner : ICommandRunner
	{
		private readonly ISharedRoot _root;
		private readonly ILogger _logger;

		public ServerCommandRunner(ISharedRoot root,
			ILogger logger = null)
		{
			_root = root;
			_logger = logger;
		}

		public readonly bool Execute<T>(in T command) where T : struct, ICommand
		{
			if (!command.Validate(_root, out var exception))
			{
				_logger?.Exception(exception);
				return false;
			}

			command.Execute(_root);
			_root.OnExecuted(in command);
			return true;
		}
	}
}
