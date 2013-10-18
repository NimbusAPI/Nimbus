namespace Nimbus
{
    public interface ICommandBroker
    {
        void Dispatch<TBusCommand>(TBusCommand busEvent);
    }
}