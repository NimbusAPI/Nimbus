using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.Commands
{
    internal class BusCommandSender : ICommandSender
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IKnownMessageTypeVerifier _knownMessageTypeVerifier;
        private readonly ILogger _logger;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;

        public BusCommandSender(IBrokeredMessageFactory brokeredMessageFactory,
                                IKnownMessageTypeVerifier knownMessageTypeVerifier,
                                ILogger logger,
                                INimbusMessagingFactory messagingFactory,
                                IRouter router)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _knownMessageTypeVerifier = knownMessageTypeVerifier;
            _logger = logger;
            _messagingFactory = messagingFactory;
            _router = router;
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
            var queuePath = _router.Route(commandType);
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