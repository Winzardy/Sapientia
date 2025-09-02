
namespace SharedLogic
{
    public interface ICommandRunner
    {
        /// <summary>
        /// Execute command and generate error if commands validation has failed.
        /// </summary>
        public void Execute(ICommand command);
    }
}
