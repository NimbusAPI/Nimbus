using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Logging;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class BusRequestSender : IRequestSender
    {
        private readonly INimbusTransport _transport;
        private readonly IRouter _router;
        private readonly INimbusMessageFactory _nimbusMessageFactory;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly ILogger _logger;
        private readonly IClock _clock;
        private readonly DefaultTimeoutSetting _responseTimeout;
        private readonly IKnownMessageTypeVerifier _knownMessageTypeVerifier;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;
        private readonly IPathFactory _pathFactory;

        internal BusRequestSender(DefaultTimeoutSetting responseTimeout,
                                  IClock clock,
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
            _outboundInterceptorFactory = outboundInterceptorFactory;
            _dependencyResolver = dependencyResolver;
            _logger = logger;
            _clock = clock;
            _responseTimeout = responseTimeout;
            _knownMessageTypeVerifier = knownMessageTypeVerifier;
        }

        public async Task<TResponse> SendRequest<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            return await SendRequest(busRequest, _responseTimeout);
        }

        public async Task<TResponse> SendRequest<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            var requestType = busRequest.GetType();
            _knownMessageTypeVerifier.AssertValidMessageType(requestType);

            var queuePath = _router.Route(requestType, QueueOrTopic.Queue, _pathFactory);

            var nimbusMessage = (await _nimbusMessageFactory.Create(queuePath, busRequest))
                .WithRequestTimeout(timeout)
                ;

            DispatchLoggingContext.NimbusMessage = nimbusMessage;

            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordRequest<TResponse>(nimbusMessage.MessageId, expiresAfter);

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
                        await interceptor.OnRequestSending(busRequest, nimbusMessage);
                    }
                    await sender.Send(nimbusMessage);
                    foreach (var interceptor in interceptors.Reverse())
                    {
                        await interceptor.OnRequestSent(busRequest, nimbusMessage);
                    }
                    _logger.LogDispatchAction("Sent", queuePath, sw.Elapsed);

                    _logger.LogDispatchAction("Waiting for response to", queuePath, sw.Elapsed);
                    var response = await responseCorrelationWrapper.WaitForResponse(timeout);
                    _logger.LogDispatchAction("Received response to", queuePath, sw.Elapsed);

                    return response;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                foreach (var interceptor in interceptors.Reverse())
                {
                    await interceptor.OnRequestSendingError(busRequest, nimbusMessage, exception);
                }
                _logger.LogDispatchError("sending", queuePath, sw.Elapsed, exception);
                //FIXME "sending" here is a bit misleading. The message could have been sent and the response not received.

                ExceptionDispatchInfo.Capture(exception).Throw();
                return default(TResponse);
            }
        }
    }
}