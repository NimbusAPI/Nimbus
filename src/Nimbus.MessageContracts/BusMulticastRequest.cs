using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.MessageContracts
{
    public abstract class BusMulticastRequest<TRequest, TResponse> : IBusMulticastRequest<TRequest, TResponse>
        where TRequest : IBusMulticastRequest<TRequest, TResponse>
        where TResponse : IBusMulticastResponse
        // ReSharper restore UnusedTypeParameter
    {
        protected BusMulticastRequest()
        {
            if (GetType() != typeof (TRequest)) throw new InvalidRequestTypeException("Bus requests must have their *own* type as their first generic type parameter");
        }
    }
}