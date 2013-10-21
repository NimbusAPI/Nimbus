namespace Nimbus
{
    public interface ICommandBroker
    {
        void Dispatch<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand;
    }
}