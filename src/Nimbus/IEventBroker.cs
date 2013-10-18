namespace Nimbus
{
    public interface IEventBroker
    {
        void Publish<TEvent>(TEvent busEvent);
    }
}