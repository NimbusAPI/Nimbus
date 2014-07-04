using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.Commands
{
    internal class BusCommandSender : ICommandSender
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IKnownMessageTypeVerifier _knownMessageTypeVerifier;
        private readonly ILogger _logger;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;

        public BusCommandSender(IBrokeredMessageFactory brokeredMessageFactory,
                                IDependencyResolver dependencyResolver,
                                IKnownMessageTypeVerifier knownMessageTypeVerifier,
                                ILogger logger,
                                INimbusMessagingFactory messagingFactory,
                                IOutboundInterceptorFactory outboundInterceptorFactory,
                                IRouter router)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _knownMessageTypeVerifier = knownMessageTypeVerifier;
            _logger = logger;
            _messagingFactory = messagingFactory;
            _router = router;
            _dependencyResolver = dependencyResolver;
            _outboundInterceptorFactory = outboundInterceptorFactory;
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            var commandType = busCommand.GetType();
            _knownMessageTypeVerifier.AssertValidMessageType(commandType);

            var message = await _brokeredMessageFactory.Create(busCommand);

            await Deliver(busCommand, commandType, message);
        }

        public async Task SendAt<TBusCommand>(TBusCommand busCommand, DateTimeOffset whenToSend) where TBusCommand : IBusCommand
        {
            var commandType = busCommand.GetType();
            _knownMessageTypeVerifier.AssertValidMessageType(commandType);

            var message = (await _brokeredMessageFactory.Create(busCommand)).WithScheduledEnqueueTime(whenToSend);

            await Deliver(busCommand, commandType, message);
        }

        private async Task Deliver<TBusCommand>(TBusCommand busCommand, Type commandType, BrokeredMessage message) where TBusCommand : IBusCommand
        {
            var queuePath = _router.Route(commandType, QueueOrTopic.Queue);
            message.DestinedForQueue(queuePath);

            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope);
                foreach (var interceptor in interceptors)
                {
                    await interceptor.OnCommandSending(busCommand, message);
                }
            }

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