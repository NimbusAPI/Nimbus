using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.Events
{
    internal class BusEventSender : IEventSender
    {
        private readonly ITopicClientFactory _topicClientFactory;
        private readonly ILogger _logger;
        private readonly HashSet<Type> _validEventTypes;

        public BusEventSender(ITopicClientFactory topicClientFactory, EventTypesSetting validEventTypes, ILogger logger)
        {
            _topicClientFactory = topicClientFactory;
            _logger = logger;
            _validEventTypes = new HashSet<Type>(validEventTypes.Value);
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent)
        {
            if (!_validEventTypes.Contains(typeof (TBusEvent)))
                throw new BusException(
                    "The type {0} is not a recognised event type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(
                        typeof (TBusEvent).FullName));

            var client = _topicClientFactory.GetTopicClient(typeof (TBusEvent));
            var brokeredMessage = new BrokeredMessage(busEvent);

            await client.SendAsync(brokeredMessage);
            _logger.Debug("Published event: {0}", brokeredMessage);
        }
    }
}