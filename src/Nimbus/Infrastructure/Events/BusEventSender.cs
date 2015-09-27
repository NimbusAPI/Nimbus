﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Logging;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.Events
{
    internal class BusEventSender : IEventSender
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly ILogger _logger;
        private readonly IKnownMessageTypeVerifier _knownMessageTypeVerifier;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;

        public BusEventSender(IBrokeredMessageFactory brokeredMessageFactory,
                              IDependencyResolver dependencyResolver,
                              IKnownMessageTypeVerifier knownMessageTypeVerifier,
                              ILogger logger,
                              INimbusMessagingFactory messagingFactory,
                              IOutboundInterceptorFactory outboundInterceptorFactory,
                              IRouter router)
        {
            _messagingFactory = messagingFactory;
            _router = router;
            _dependencyResolver = dependencyResolver;
            _outboundInterceptorFactory = outboundInterceptorFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _logger = logger;
            _knownMessageTypeVerifier = knownMessageTypeVerifier;
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            var eventType = busEvent.GetType();

            _knownMessageTypeVerifier.AssertValidMessageType(eventType);

            var brokeredMessage = await _brokeredMessageFactory.Create(busEvent);
            var topicPath = _router.Route(eventType, QueueOrTopic.Topic);

            using (var scope = _dependencyResolver.CreateChildScope())
            {
                Exception exception;

                var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope, brokeredMessage);
                try
                {
                    _logger.LogDispatchAction("Publishing", topicPath, brokeredMessage);

                    var topicSender = _messagingFactory.GetTopicSender(topicPath);
                    foreach (var interceptor in interceptors)
                    {
                        await interceptor.OnEventPublishing(busEvent, brokeredMessage);
                    }
                    await topicSender.Send(brokeredMessage);
                    foreach (var interceptor in interceptors.Reverse())
                    {
                        await interceptor.OnEventPublished(busEvent, brokeredMessage);
                    }
                    _logger.LogDispatchAction("Published", topicPath, brokeredMessage);

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
                _logger.LogDispatchError("publishing", topicPath, brokeredMessage, exception);
            }
        }

        public async Task PublishAt<TBusEvent>(TBusEvent busEvent, DateTimeOffset deliveryTime) where TBusEvent : IBusEvent
        {
            var eventType = busEvent.GetType();

            _knownMessageTypeVerifier.AssertValidMessageType(eventType);

            var brokeredMessage = (await _brokeredMessageFactory.Create(busEvent)).WithScheduledEnqueueTime(deliveryTime);
            var topicPath = _router.Route(eventType, QueueOrTopic.Topic);

            using (var scope = _dependencyResolver.CreateChildScope())
            {
                Exception exception;

                var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope, brokeredMessage);
                try
                {
                    _logger.LogDispatchAction("Publishing", topicPath, brokeredMessage);

                    var topicSender = _messagingFactory.GetTopicSender(topicPath);
                    foreach (var interceptor in interceptors)
                    {
                        await interceptor.OnEventPublishing(busEvent, brokeredMessage);
                    }
                    await topicSender.Send(brokeredMessage);
                    foreach (var interceptor in interceptors.Reverse())
                    {
                        await interceptor.OnEventPublished(busEvent, brokeredMessage);
                    }
                    _logger.LogDispatchAction("Published", topicPath, brokeredMessage);

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
                _logger.LogDispatchError("publishing", topicPath, brokeredMessage, exception);
            }
        }
    }
}