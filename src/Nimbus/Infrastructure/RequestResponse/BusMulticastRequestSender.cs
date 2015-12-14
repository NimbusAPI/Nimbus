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
        private readonly INimbusTransport _transport;
        private readonly IRouter _router;
        private readonly INimbusMessageFactory _nimbusMessageFactory;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly IKnownMessageTypeVerifier _knownMessageTypeVerifier;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;

        public BusMulticastRequestSender(IClock clock,
                                         IDependencyResolver dependencyResolver,
                                         IKnownMessageTypeVerifier knownMessageTypeVerifier,
                                         ILogger logger,
                                         INimbusMessageFactory nimbusMessageFactory,
                                         INimbusTransport transport,
                                         IOutboundInterceptorFactory outboundInterceptorFactory,
                                         IRouter router,
                                         RequestResponseCorrelator requestResponseCorrelator)
        {
            _transport = transport;
            _router = router;
            _nimbusMessageFactory = nimbusMessageFactory;
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

            var brokeredMessage = (await _nimbusMessageFactory.Create(busRequest))
                .WithRequestTimeout(timeout)
                .DestinedForTopic(topicPath)
                ;
            var expiresAfter = _clock.UtcNow.AddSafely(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordMulticastRequest<TResponse>(brokeredMessage.MessageId, expiresAfter);

            using (var scope = _dependencyResolver.CreateChildScope())
            {
                Exception exception;

                var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope, brokeredMessage);
                try
                {
                    _logger.LogDispatchAction("Sending", topicPath, brokeredMessage);

                    var sender = _transport.GetTopicSender(topicPath);
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