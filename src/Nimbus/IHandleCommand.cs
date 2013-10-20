namespace Nimbus
{
    public interface IHandleCommand<TBusCommand> where TBusCommand : IBusCommand
    {
        void Handle(TBusCommand busCommand);
    }
}