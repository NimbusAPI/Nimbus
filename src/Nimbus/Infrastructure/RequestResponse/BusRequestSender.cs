using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class BusRequestSender : IRequestSender
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly ILogger _logger;
        private readonly IClock _clock;
        private readonly DefaultTimeoutSetting _responseTimeout;
        private readonly IKnownMessageTypeVerifier _knownMessageTypeVerifier;

        internal BusRequestSender(DefaultTimeoutSetting responseTimeout,
                                  IBrokeredMessageFactory brokeredMessageFactory,
                                  IClock clock,
                                  IKnownMessageTypeVerifier knownMessageTypeVerifier,
                                  ILogger logger,
                                  INimbusMessagingFactory messagingFactory,
                                  IRouter router,
                                  RequestResponseCorrelator requestResponseCorrelator)
        {
            _messagingFactory = messagingFactory;
            _router = router;
            _brokeredMessageFactory = brokeredMessageFactory;
            _requestResponseCorrelator = requestResponseCorrelator;
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

            var message = (await _brokeredMessageFactory.Create(busRequest)).WithRequestTimeout(timeout);

            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordRequest<TResponse>(Guid.Parse(message.CorrelationId), expiresAfter);

            var queuePath = _router.Route(requestType);
            var sender = _messagingFactory.GetQueueSender(queuePath);

            _logger.Debug("Sending request {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                          message.SafelyGetBodyTypeNameOrDefault(),
                          queuePath,
                          message.MessageId,
                          message.CorrelationId);
            await sender.Send(message);
            _logger.Info("Sent request {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                         message.SafelyGetBodyTypeNameOrDefault(),
                         queuePath,
                         message.MessageId,
                         message.CorrelationId);

            _logger.Debug("Waiting for response to {0} from {1} [MessageId:{2}, CorrelationId:{3}]",
                          message.SafelyGetBodyTypeNameOrDefault(),
                          queuePath,
                          message.MessageId,
                          message.CorrelationId);
            var response = await responseCorrelationWrapper.WaitForResponse(timeout);
            _logger.Info("Received response to {0} from {1} [MessageId:{2}, CorrelationId:{3}] in the form of {4}",
                         message.SafelyGetBodyTypeNameOrDefault(),
                         queuePath,
                         message.MessageId,
                         message.CorrelationId,
                         response.GetType().FullName);

            return response;
        }
    }
}
