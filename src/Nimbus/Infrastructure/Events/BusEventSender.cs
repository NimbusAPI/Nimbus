using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Infrastructure.Logging;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.Events
{
    internal class BusEventSender : IEventSender
    {
        private readonly INimbusTransport _transport;
        private readonly IRouter _router;
        private readonly INimbusMessageFactory _nimbusMessageFactory;
        private readonly ILogger _logger;
        private readonly IKnownMessageTypeVerifier _knownMessageTypeVerifier;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;
        private readonly IPathFactory _pathFactory;

        public BusEventSender(IDependencyResolver dependencyResolver,
                              IKnownMessageTypeVerifier knownMessageTypeVerifier,
                              ILogger logger,
                              INimbusMessageFactory nimbusMessageFactory,
                              INimbusTransport transport,
                              IOutboundInterceptorFactory outboundInterceptorFactory,
                              IPathFactory pathFactory,
                              IRouter router)
        {
            _transport = transport;
            _router = router;
            _pathFactory = pathFactory;
            _dependencyResolver = dependencyResolver;
            _outboundInterceptorFactory = outboundInterceptorFactory;
            _nimbusMessageFactory = nimbusMessageFactory;
            _logger = logger;
            _knownMessageTypeVerifier = knownMessageTypeVerifier;
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            var eventType = busEvent.GetType();

            _knownMessageTypeVerifier.AssertValidMessageType(eventType);

            var topicPath = _router.Route(eventType, QueueOrTopic.Topic, _pathFactory);
            var brokeredMessage = await _nimbusMessageFactory.Create(topicPath, busEvent);

            var sw = Stopwatch.StartNew();
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                Exception exception;

                var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope, brokeredMessage);
                try
                {
                    _logger.LogDispatchAction("Publishing", topicPath, sw.Elapsed);

                    var topicSender = _transport.GetTopicSender(topicPath);
                    foreach (var interceptor in interceptors)
                    {
                        await interceptor.OnEventPublishing(busEvent, brokeredMessage);
                    }
                    await topicSender.Send(brokeredMessage);
                    foreach (var interceptor in interceptors.Reverse())
                    {
                        await interceptor.OnEventPublished(busEvent, brokeredMessage);
                    }
                    _logger.LogDispatchAction("Published", topicPath, sw.Elapsed);

                    return;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                foreach (var interceptor in interceptors.Reverse())
                {
                    await interceptor.OnEventPublishingError(busEvent, brokeredMessage, exception);
                }
                _logger.LogDispatchError("publishing", topicPath, sw.Elapsed, exception);
            }
        }
    }
}