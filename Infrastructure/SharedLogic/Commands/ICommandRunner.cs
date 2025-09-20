namespace SharedLogic
{
	public interface ICommandRunner
	{
		public long Timestamp { get; }
		public void Execute<T>(in T command) where T : struct, ICommand;
	}

	public interface ICommandSender
	{

	}
}
