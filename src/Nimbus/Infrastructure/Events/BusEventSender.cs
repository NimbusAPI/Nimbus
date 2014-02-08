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
        private readonly INimbusMessageSenderFactory _messageSenderFactory;
        private readonly ILogger _logger;
        private readonly HashSet<Type> _validEventTypes;

        public BusEventSender(INimbusMessageSenderFactory messageSenderFactory, EventTypesSetting validEventTypes, ILogger logger)
        {
            _messageSenderFactory = messageSenderFactory;
            _logger = logger;
            _validEventTypes = new HashSet<Type>(validEventTypes.Value);
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent)
        {
            AssertValidEventType<TBusEvent>();

            var brokeredMessage = new BrokeredMessage(busEvent);

            var client = _messageSenderFactory.GetTopicSender(typeof (TBusEvent));
            await client.Send(brokeredMessage);

            _logger.Debug("Published event: {0}", brokeredMessage);
        }

        private void AssertValidEventType<TBusEvent>()
        {
            if (!_validEventTypes.Contains(typeof (TBusEvent)))
                throw new BusException(
                    "The type {0} is not a recognised event type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(
                        typeof (TBusEvent).FullName));
        }
    }
}