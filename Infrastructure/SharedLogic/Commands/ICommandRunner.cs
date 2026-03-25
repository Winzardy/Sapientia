namespace SharedLogic
{
	public interface ICommandRunner
	{
		void Execute<T>(in T command) where T : struct, ICommand;
		bool IsEmpty { get; }
	}
}
