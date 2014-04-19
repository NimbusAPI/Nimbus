using Nimbus.MessageContracts;

namespace Nimbus.Interceptors
{
    public interface IEventInterceptor<in TBusEvent> : IMessageInterceptor<TBusEvent> where TBusEvent : IBusEvent
    {
    }
}