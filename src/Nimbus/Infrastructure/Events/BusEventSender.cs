using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.Events
{
    internal class BusEventSender : IEventSender
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly ILogger _logger;
        private readonly HashSet<Type> _validEventTypes;

        public BusEventSender(
            INimbusMessagingFactory messagingFactory,
            IBrokeredMessageFactory brokeredMessageFactory,
            EventTypesSetting validEventTypes,
            ILogger logger)
        {
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _logger = logger;
            _validEventTypes = new HashSet<Type>(validEventTypes.Value);
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent)
        {
            var eventType = busEvent.GetType();
            AssertValidEventType(eventType);

            var brokeredMessage = _brokeredMessageFactory.Create(busEvent);

            var client = _messagingFactory.GetTopicSender(PathFactory.TopicPathFor(eventType));
            await client.Send(brokeredMessage);

            _logger.Debug("Published event: {0}", busEvent);
        }

        private void AssertValidEventType(Type eventType)
        {
            if (!_validEventTypes.Contains(eventType))
                throw new BusException(
                    "The type {0} is not a recognised event type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(
                        eventType.FullName));
        }
    }
}