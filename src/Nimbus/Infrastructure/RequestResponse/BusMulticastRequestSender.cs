using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IPathFactory _pathFactory;

        public BusMulticastRequestSender(IClock clock,
                                         IDependencyResolver dependencyResolver,
                                         IKnownMessageTypeVerifier knownMessageTypeVerifier,
                                         ILogger logger,
                                         INimbusMessageFactory nimbusMessageFactory,
                                         INimbusTransport transport,
                                         IOutboundInterceptorFactory outboundInterceptorFactory,
                                         IPathFactory pathFactory,
                                         IRouter router,
                                         RequestResponseCorrelator requestResponseCorrelator)
        {
            _transport = transport;
            _router = router;
            _nimbusMessageFactory = nimbusMessageFactory;
            _requestResponseCorrelator = requestResponseCorrelator;
            _pathFactory = pathFactory;
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

            var topicPath = _router.Route(requestType, QueueOrTopic.Topic, _pathFactory);

            var nimbusMessage = (await _nimbusMessageFactory.Create(topicPath, busRequest))
                .WithRequestTimeout(timeout)
                .DestinedForTopic(topicPath)
                ;
            DispatchLoggingContext.NimbusMessage = nimbusMessage;
            var expiresAfter = _clock.UtcNow.AddSafely(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordMulticastRequest<TResponse>(nimbusMessage.MessageId, expiresAfter);

            var sw = Stopwatch.StartNew();
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                Exception exception;

                var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope, nimbusMessage);
                try
                {
                    _logger.LogDispatchAction("Sending", topicPath, sw.Elapsed);

                    var sender = _transport.GetTopicSender(topicPath);
                    foreach (var interceptor in interceptors)
                    {
                        await interceptor.OnMulticastRequestSending(busRequest, nimbusMessage);
                    }
                    await sender.Send(nimbusMessage);
                    foreach (var interceptor in interceptors.Reverse())
                    {
                        await interceptor.OnMulticastRequestSent(busRequest, nimbusMessage);
                    }

                    _logger.LogDispatchAction("Sent", topicPath, sw.Elapsed);

                    _logger.LogDispatchAction("Waiting for responses to", topicPath, sw.Elapsed);
                    var responsesEnumerable = responseCorrelationWrapper.ReturnResponsesOpportunistically(timeout);
                    return responsesEnumerable;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                foreach (var interceptor in interceptors.Reverse())
                {
                    await interceptor.OnMulticastRequestSendingError(busRequest, nimbusMessage, exception);
                }
                _logger.LogDispatchError("sending", topicPath, sw.Elapsed, exception);

                ExceptionDispatchInfo.Capture(exception).Throw();
                return null;
            }
        }
    }
}