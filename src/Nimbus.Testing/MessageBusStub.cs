using Nimbus.MessageContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nimbus.Testing
{
    public class MessageBusStub : IBus
    {
        public List<IBusEvent> BusEvents { get; }

        public IDeadLetterQueues DeadLetterQueues => throw new NotImplementedException();

        public MessageBusStub()
        {
            BusEvents = new List<IBusEvent>();
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            throw new NotImplementedException();
        }

        public async Task SendAt<TBusCommand>(TBusCommand busCommand, DateTimeOffset deliveryTime) where TBusCommand : IBusCommand
        {
            throw new NotImplementedException();
        }

        public async Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            throw new NotImplementedException();
        }

        public async Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TResponse>> MulticastRequest<TRequest, TResponse>(IBusMulticastRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusMulticastRequest<TRequest, TResponse>
            where TResponse : IBusMulticastResponse
        {
            throw new NotImplementedException();
        }

        public virtual async Task Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            BusEvents.Add(busEvent);
        }
    }
}