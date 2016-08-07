using System;
using System.Diagnostics;
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
        private readonly INimbusMessageFactory _nimbusMessageFactory;
        private readonly INimbusTransport _transport;
        private readonly IRouter _router;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;
        private readonly IPathFactory _pathFactory;

        public BusCommandSender(IDependencyResolver dependencyResolver,
                                IKnownMessageTypeVerifier knownMessageTypeVerifier,
                                ILogger logger,
                                INimbusMessageFactory nimbusMessageFactory,
                                INimbusTransport transport,
                                IOutboundInterceptorFactory outboundInterceptorFactory,
                                IPathFactory pathFactory,
                                IRouter router)
        {
            _nimbusMessageFactory = nimbusMessageFactory;
            _knownMessageTypeVerifier = knownMessageTypeVerifier;
            _logger = logger;
            _transport = transport;
            _router = router;
            _pathFactory = pathFactory;
            _dependencyResolver = dependencyResolver;
            _outboundInterceptorFactory = outboundInterceptorFactory;
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            var commandType = busCommand.GetType();
            _knownMessageTypeVerifier.AssertValidMessageType(commandType);

            var destinationPath = _router.Route(commandType, QueueOrTopic.Queue, _pathFactory);
            var message = await _nimbusMessageFactory.Create(destinationPath, busCommand);

            await Deliver(busCommand, commandType, message);
        }

        public async Task SendAt<TBusCommand>(TBusCommand busCommand, DateTimeOffset whenToSend) where TBusCommand : IBusCommand
        {
            var commandType = busCommand.GetType();
            _knownMessageTypeVerifier.AssertValidMessageType(commandType);

            var destinationPath = _router.Route(commandType, QueueOrTopic.Queue, _pathFactory);
            var message = (await _nimbusMessageFactory.Create(destinationPath, busCommand)).WithScheduledEnqueueTime(whenToSend);

            await Deliver(busCommand, commandType, message);
        }

        private async Task Deliver<TBusCommand>(TBusCommand busCommand, Type commandType, NimbusMessage nimbusMessage) where TBusCommand : IBusCommand
        {
            var queuePath = _router.Route(commandType, QueueOrTopic.Queue, _pathFactory);
            nimbusMessage.DestinedForQueue(queuePath);

            var sw = Stopwatch.StartNew();
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                Exception exception;

                var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope, nimbusMessage);
                try
                {
                    _logger.LogDispatchAction("Sending", queuePath, sw.Elapsed);

                    var sender = _transport.GetQueueSender(queuePath);
                    foreach (var interceptor in interceptors)
                    {
                        await interceptor.OnCommandSending(busCommand, nimbusMessage);
                    }
                    await sender.Send(nimbusMessage);
                    foreach (var interceptor in interceptors.Reverse())
                    {
                        await interceptor.OnCommandSent(busCommand, nimbusMessage);
                    }

                    _logger.LogDispatchAction("Sent", queuePath, sw.Elapsed);
                    return;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                foreach (var interceptor in interceptors.Reverse())
                {
                    await interceptor.OnCommandSendingError(busCommand, nimbusMessage, exception);
                }
                _logger.LogDispatchError("sending", queuePath, sw.Elapsed, exception);
            }
        }
    }
}