using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.Commands
{
    internal class BusCommandSender : ICommandSender
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly ILogger _logger;
        private readonly HashSet<Type> _validCommandTypes;

        public BusCommandSender(
            INimbusMessagingFactory messagingFactory,
            IBrokeredMessageFactory brokeredMessageFactory,
            CommandTypesSetting validCommandTypes,
            ILogger logger)
        {
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _logger = logger;
            _validCommandTypes = new HashSet<Type>(validCommandTypes.Value);
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            var commandType = busCommand.GetType();
            AssertValidCommandType(commandType);

            var message = await _brokeredMessageFactory.Create(busCommand);

            await Deliver(commandType, message);
        }

        public async Task SendAt<TBusCommand>(TBusCommand busCommand, DateTimeOffset whenToSend)
        {
            var commandType = busCommand.GetType();
            AssertValidCommandType(commandType);

            var message = (await _brokeredMessageFactory.Create(busCommand)).WithScheduledEnqueueTime(whenToSend);

            await Deliver(commandType, message);
        }

        private void AssertValidCommandType(Type commandType)
        {
            if (!_validCommandTypes.Contains(commandType))
                throw new BusException(
                    "The type {0} is not a recognised command type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(
                        commandType.FullName));
        }

        private async Task Deliver(Type commandType, BrokeredMessage message)
        {
            var queuePath = PathFactory.QueuePathFor(commandType);
            var sender = _messagingFactory.GetQueueSender(queuePath);

            _logger.Debug("Sending command {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                message.SafelyGetBodyTypeNameOrDefault(), queuePath, message.MessageId, message.CorrelationId);
            await sender.Send(message);
            _logger.Debug("Sent command {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                message.SafelyGetBodyTypeNameOrDefault(), queuePath, message.MessageId, message.CorrelationId);
        }
    }
}