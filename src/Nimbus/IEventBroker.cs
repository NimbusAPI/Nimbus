namespace Nimbus
{
    public interface IEventBroker
    {
        void Publish<TBusEvent>(TBusEvent busEvent);
    }
}