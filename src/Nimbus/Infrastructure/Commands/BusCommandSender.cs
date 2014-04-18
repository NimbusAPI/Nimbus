using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.Commands
{
    internal class BusCommandSender : ICommandSender
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly ILogger _logger;
        private readonly IKnownMessageTypeVerifier _knownMessageTypeVerifier;

        public BusCommandSender(
            INimbusMessagingFactory messagingFactory,
            IBrokeredMessageFactory brokeredMessageFactory,
            ILogger logger,
            IKnownMessageTypeVerifier knownMessageTypeVerifier)
        {
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _logger = logger;
            _knownMessageTypeVerifier = knownMessageTypeVerifier;
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            var commandType = busCommand.GetType();

            _knownMessageTypeVerifier.AssertValidMessageType(commandType);

            var message = await _brokeredMessageFactory.Create(busCommand);

            await Deliver(commandType, message);
        }

        public async Task SendAt<TBusCommand>(TBusCommand busCommand, DateTimeOffset whenToSend)
        {
            var commandType = busCommand.GetType();
            _knownMessageTypeVerifier.AssertValidMessageType(commandType);

            var message = (await _brokeredMessageFactory.Create(busCommand)).WithScheduledEnqueueTime(whenToSend);

            await Deliver(commandType, message);
        }

        private async Task Deliver(Type commandType, BrokeredMessage message)
        {
            var queuePath = PathFactory.QueuePathFor(commandType);
            var sender = _messagingFactory.GetQueueSender(queuePath);

            _logger.Debug("Sending command {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                          message.SafelyGetBodyTypeNameOrDefault(),
                          queuePath,
                          message.MessageId,
                          message.CorrelationId);
            await sender.Send(message);
            _logger.Debug("Sent command {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                          message.SafelyGetBodyTypeNameOrDefault(),
                          queuePath,
                          message.MessageId,
                          message.CorrelationId);
        }
    }
}