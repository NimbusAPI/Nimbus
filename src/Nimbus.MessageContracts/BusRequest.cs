using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.MessageContracts
{
    // ReSharper disable UnusedTypeParameter
    public abstract class BusRequest<TRequest, TResponse> : IBusRequest<TRequest, TResponse>
        where TRequest : IBusRequest<TRequest, TResponse>
        where TResponse : IBusResponse
        // ReSharper restore UnusedTypeParameter
    {
        protected BusRequest()
        {
            if (GetType() != typeof (TRequest)) throw new InvalidRequestTypeException("Bus requests must have their *own* type as their first generic type parameter");
        }
    }
}