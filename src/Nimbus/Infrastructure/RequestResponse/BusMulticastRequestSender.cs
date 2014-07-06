using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
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
                                         IRouter router,
                                         RequestResponseCorrelator requestResponseCorrelator)
        {
            _messagingFactory = messagingFactory;
            _router = router;
            _brokeredMessageFactory = brokeredMessageFactory;
            _requestResponseCorrelator = requestResponseCorrelator;
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

            var topicPath = _router.Route(requestType, QueueOrTopic.Topic);

            var message = (await _brokeredMessageFactory.Create(busRequest))
                .WithRequestTimeout(timeout)
                .DestinedForTopic(topicPath)
                ;
            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordMulticastRequest<TResponse>(Guid.Parse(message.MessageId), expiresAfter);

            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope);
                foreach (var interceptor in interceptors)
                {
                    await interceptor.OnMulticastRequestSending(busRequest, message);
                }
            }

            var sender = _messagingFactory.GetTopicSender(topicPath);

            _logger.Debug("Sending multicast request {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                          message.SafelyGetBodyTypeNameOrDefault(),
                          topicPath,
                          message.MessageId,
                          message.CorrelationId);
            await sender.Send(message);
            _logger.Info("Sent multicast request {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                         message.SafelyGetBodyTypeNameOrDefault(),
                         topicPath,
                         message.MessageId,
                         message.CorrelationId);

            _logger.Debug("Waiting for multicast response to {0} from {1} [MessageId:{2}, CorrelationId:{3}]",
                          message.SafelyGetBodyTypeNameOrDefault(),
                          topicPath,
                          message.MessageId,
                          message.CorrelationId);
            var response = responseCorrelationWrapper.ReturnResponsesOpportunistically(timeout);
            _logger.Info("Received response to {0} from {1} [MessageId:{2}, CorrelationId:{3}] in the form of {4}",
                         message.SafelyGetBodyTypeNameOrDefault(),
                         topicPath,
                         message.MessageId,
                         message.CorrelationId,
                         response.GetType().FullName);

            return response;
        }
    }
}