using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Logging;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class BusMulticastRequestSender : IMulticastRequestSender
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly IPathGenerator _pathGenerator;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly IKnownMessageTypeVerifier _knownMessageTypeVerifier;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;

        public BusMulticastRequestSender(IBrokeredMessageFactory brokeredMessageFactory,
                                         IClock clock,
                                         IDependencyResolver dependencyResolver,
                                         IKnownMessageTypeVerifier knownMessageTypeVerifier,
                                         ILogger logger,
                                         INimbusMessagingFactory messagingFactory,
                                         IOutboundInterceptorFactory outboundInterceptorFactory,
                                         IPathGenerator pathGenerator,
                                         IRouter router,
                                         RequestResponseCorrelator requestResponseCorrelator)
        {
            _messagingFactory = messagingFactory;
            _router = router;
            _brokeredMessageFactory = brokeredMessageFactory;
            _requestResponseCorrelator = requestResponseCorrelator;
            _pathGenerator = pathGenerator;
            _dependencyResolver = dependencyResolver;
            _outboundInterceptorFactory = outboundInterceptorFactory;
            _clock = clock;
            _logger = logger;
            _knownMessageTypeVerifier = knownMessageTypeVerifier;
        }

        public async Task<IEnumerable<TResponse>> SendRequest<TRequest, TResponse>(IBusMulticastRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusMulticastRequest<TRequest, TResponse>
            where TResponse : IBusMulticastResponse
        {
            var requestType = busRequest.GetType();
            _knownMessageTypeVerifier.AssertValidMessageType(requestType);

            var topicPath = _router.Route(requestType, QueueOrTopic.Topic, _pathGenerator);

            var brokeredMessage = (await _brokeredMessageFactory.Create(busRequest))
                .WithRequestTimeout(timeout)
                .DestinedForTopic(topicPath)
                ;
            var expiresAfter = _clock.UtcNow.AddSafely(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordMulticastRequest<TResponse>(Guid.Parse(brokeredMessage.MessageId), expiresAfter);

            using (var scope = _dependencyResolver.CreateChildScope())
            {
                Exception exception;

                var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope, brokeredMessage);
                try
                {
                    _logger.LogDispatchAction("Sending", topicPath, brokeredMessage);

                    var sender = _messagingFactory.GetTopicSender(topicPath);
                    foreach (var interceptor in interceptors)
                    {
                        await interceptor.OnMulticastRequestSending(busRequest, brokeredMessage);
                    }
                    await sender.Send(brokeredMessage);
                    foreach (var interceptor in interceptors.Reverse())
                    {
                        await interceptor.OnMulticastRequestSent(busRequest, brokeredMessage);
                    }

                    _logger.LogDispatchAction("Sent", topicPath, brokeredMessage);

                    _logger.LogDispatchAction("Waiting for response to", topicPath, brokeredMessage);
                    var response = responseCorrelationWrapper.ReturnResponsesOpportunistically(timeout);
                    _logger.LogDispatchAction("Received response to", topicPath, brokeredMessage);

                    return response;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                foreach (var interceptor in interceptors.Reverse())
                {
                    await interceptor.OnMulticastRequestSendingError(busRequest, brokeredMessage, exception);
                }
                _logger.LogDispatchError("sending", topicPath, brokeredMessage, exception);

                ExceptionDispatchInfo.Capture(exception).Throw();
                return null;
            }
        }
    }
}