namespace SharedLogic
{
	public interface ICommandRunner
	{
		public bool Execute<T>(in T command) where T : struct, ICommand;
	}

	public interface ICommandSender
	{
		public void Send<T>(in T command) where T : struct, ICommand;
	}

	public interface ICommandSenderFactory
	{
		ICommandSender Create();
	}
}
