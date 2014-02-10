using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.MessageContracts;
using Nimbus.PoisonMessages;

// ReSharper disable CheckNamespace
namespace Nimbus
// ReSharper restore CheckNamespace
{
    public interface IBus
    {
        Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand;

        Task Defer<TBusCommand>(TimeSpan delay, TBusCommand busCommand) where TBusCommand : IBusCommand;

        Task Defer<TBusCommand>(DateTimeOffset processAt, TBusCommand busCommand) where TBusCommand : IBusCommand;

        Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse;

        Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse;

        Task<IEnumerable<TResponse>> MulticastRequest<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse;

        Task Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent;

        IDeadLetterQueues DeadLetterQueues { get; }
    }
}