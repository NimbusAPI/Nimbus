using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Logging;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.Commands
{
    internal class BusCommandSender : ICommandSender
    {
        private readonly IKnownMessageTypeVerifier _knownMessageTypeVerifier;
        private readonly ILogger _logger;
        private readonly INimbusMessageFactory _brokeredMessageFactory;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;

        public BusCommandSender(IDependencyResolver dependencyResolver,
                                IKnownMessageTypeVerifier knownMessageTypeVerifier,
                                ILogger logger,
                                INimbusMessageFactory brokeredMessageFactory,
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

        private async Task Deliver<TBusCommand>(TBusCommand busCommand, Type commandType, NimbusMessage brokeredMessage) where TBusCommand : IBusCommand
        {
            var queuePath = _router.Route(commandType, QueueOrTopic.Queue);
            brokeredMessage.DestinedForQueue(queuePath);

            using (var scope = _dependencyResolver.CreateChildScope())
            {
                Exception exception;

                var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope, brokeredMessage);
                try
                {
                    _logger.LogDispatchAction("Sending", queuePath, brokeredMessage);

                    var sender = _messagingFactory.GetQueueSender(queuePath);
                    foreach (var interceptor in interceptors)
                    {
                        await interceptor.OnCommandSending(busCommand, brokeredMessage);
                    }
                    await sender.Send(brokeredMessage);
                    foreach (var interceptor in interceptors.Reverse())
                    {
                        await interceptor.OnCommandSent(busCommand, brokeredMessage);
                    }

                    _logger.LogDispatchAction("Sent", queuePath, brokeredMessage);
                    return;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                foreach (var interceptor in interceptors.Reverse())
                {
                    await interceptor.OnCommandSendingError(busCommand, brokeredMessage, exception);
                }
                _logger.LogDispatchError("sending", queuePath, brokeredMessage, exception);
            }
        }
    }
}