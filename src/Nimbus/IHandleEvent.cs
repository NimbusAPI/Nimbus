namespace Nimbus
{
    public interface IHandleEvent<TBusEvent> where TBusEvent : IBusEvent
    {
        void Handle(TBusEvent busEvent);

    }
}