namespace SharedLogic
{
    public interface ICommandSender
    {
        void SendCommand(ICommand command);
    }
}
