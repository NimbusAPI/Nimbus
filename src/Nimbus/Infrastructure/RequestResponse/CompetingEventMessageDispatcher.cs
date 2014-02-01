using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class CompetingEventMessageDispatcher : IMessageDispatcher
    {
        private readonly ICompetingEventBroker _competingEventBroker;
        private readonly Type _eventType;

        public CompetingEventMessageDispatcher(ICompetingEventBroker competingEventBroker, Type eventType)
        {
            _competingEventBroker = competingEventBroker;
            _eventType = eventType;
        }

        public Task Dispatch(BrokeredMessage message)
        {
            return
                Task.Run(() =>
                         {
                             var busEvent = message.GetBody(_eventType);
                             _competingEventBroker.PublishCompeting((dynamic) busEvent);
                         });
        }
    }
}