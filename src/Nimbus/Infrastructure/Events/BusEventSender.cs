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

            var message = _brokeredMessageFactory.Create(busEvent);
            var topicPath = PathFactory.TopicPathFor(eventType);
            var topicSender = _messagingFactory.GetTopicSender(topicPath);

            LogActivity("Publishing event", message, topicPath);
            await topicSender.Send(message);
            LogActivity("Published event", message, topicPath);
        }

        private void LogActivity(string activity, BrokeredMessage message, string path)
        {
            _logger.Debug("{0} {1} to {2} [MessageId:{3}, CorrelationId:{4}]",
                activity, message.SafelyGetBodyTypeNameOrDefault(), path, message.MessageId, message.CorrelationId);
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