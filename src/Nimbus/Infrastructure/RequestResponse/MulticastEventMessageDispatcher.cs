using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class MulticastEventMessageDispatcher : IMessageDispatcher
    {
        private readonly IMulticastEventBroker _multicastEventBroker;
        private readonly Type _eventType;

        public MulticastEventMessageDispatcher(IMulticastEventBroker multicastEventBroker, Type eventType)
        {
            _multicastEventBroker = multicastEventBroker;
            _eventType = eventType;
        }

        public Task Dispatch(BrokeredMessage message)
        {
            return Task.Run(() =>
                            {
                                var busEvent = message.GetBody(_eventType);
                                _multicastEventBroker.PublishMulticast((dynamic) busEvent);
                            });
        }
    }
}