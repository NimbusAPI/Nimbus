using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class BusRequestSender : IRequestSender
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly ILogger _logger;
        private readonly IClock _clock;
        private readonly DefaultTimeoutSetting _responseTimeout;
        private readonly RequestTypesSetting _requestTypes;
        private readonly Lazy<HashSet<Type>> _validRequestTypes;

        internal BusRequestSender(INimbusMessagingFactory messagingFactory,
                                  IBrokeredMessageFactory brokeredMessageFactory,
                                  RequestResponseCorrelator requestResponseCorrelator,
                                  IClock clock,
                                  DefaultTimeoutSetting responseTimeout,
                                  RequestTypesSetting requestTypes,
                                  ILogger logger)
        {
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _requestResponseCorrelator = requestResponseCorrelator;
            _logger = logger;
            _clock = clock;
            _responseTimeout = responseTimeout;
            _requestTypes = requestTypes;

            _validRequestTypes = new Lazy<HashSet<Type>>(() => new HashSet<Type>(_requestTypes.Value));
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
            AssertValidRequestType<TRequest, TResponse>();

            var message = _brokeredMessageFactory.Create(busRequest).WithRequestTimeout(timeout);

            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordRequest<TResponse>(Guid.Parse(message.CorrelationId), expiresAfter);

            var queuePath = PathFactory.QueuePathFor(busRequest.GetType());
            var sender = _messagingFactory.GetQueueSender(queuePath);

            _logger.Debug("Sending request {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                message.SafelyGetBodyTypeNameOrDefault(), queuePath, message.MessageId, message.CorrelationId);
            await sender.Send(message);
            _logger.Info("Sent request {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                message.SafelyGetBodyTypeNameOrDefault(), queuePath, message.MessageId, message.CorrelationId);

            _logger.Debug("Waiting for response to {0} from {1} [MessageId:{2}, CorrelationId:{3}]",
                message.SafelyGetBodyTypeNameOrDefault(), queuePath, message.MessageId, message.CorrelationId);
            var response = responseCorrelationWrapper.WaitForResponse(timeout);
            _logger.Info("Received response to {0} from {1} [MessageId:{2}, CorrelationId:{3}] in the form of {4}",
                message.SafelyGetBodyTypeNameOrDefault(), queuePath, message.MessageId, message.CorrelationId, response.GetType().FullName);

            return response;
        }

        private void AssertValidRequestType<TRequest, TResponse>() where TRequest : IBusRequest<TRequest, TResponse> where TResponse : IBusResponse
        {
            if (!_validRequestTypes.Value.Contains(typeof (TRequest)))
                throw new BusException(
                    "The type {0} is not a recognised request type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(
                        typeof (TRequest).FullName));
        }
    }
}