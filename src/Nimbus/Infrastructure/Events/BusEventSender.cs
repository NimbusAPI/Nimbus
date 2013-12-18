using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.Events
{
    internal class BusEventSender : IEventSender
    {
        private readonly ITopicClientFactory _topicClientFactory;
        private readonly HashSet<Type> _validEventTypes;

        public BusEventSender(ITopicClientFactory topicClientFactory, IReadOnlyList<Type> validEventTypes)
        {
            _topicClientFactory = topicClientFactory;
            _validEventTypes = new HashSet<Type>(validEventTypes);
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent)
        {
            if (!_validEventTypes.Contains(typeof (TBusEvent)))
                throw new BusException("The type {0} is not a recognised event type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(typeof(TBusEvent).FullName));

            var client = _topicClientFactory.GetTopicClient(typeof (TBusEvent));
            var brokeredMessage = new BrokeredMessage(busEvent);
            await client.SendBatchAsync(new[] {brokeredMessage});
        }
    }
}