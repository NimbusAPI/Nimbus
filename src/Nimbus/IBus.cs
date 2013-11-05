using System;
using System.Threading.Tasks;
using Nimbus.MessageContracts;
using Nimbus.PoisonMessages;

namespace Nimbus
{
    public interface IBus
    {
        Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand;

        Task Defer<TBusTimeout>(DateTime proccessAt, TBusTimeout busTimeout) where TBusTimeout : IBusTimeout;

        Task Defer<TBusTimeout>(TimeSpan delay, TBusTimeout busTimeout) where TBusTimeout : IBusTimeout;

        Task<TResponse> Request<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
            where TRequest : IBusRequest
            where TResponse : IBusResponse;

        Task Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent;

        IDeadLetterQueues DeadLetterQueues { get; }
    }
}