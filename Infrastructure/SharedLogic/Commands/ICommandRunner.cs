namespace SharedLogic
{
	public interface ICommandRunner
	{
		public bool Execute<T>(in T command) where T : struct, ICommand;
	}
}
