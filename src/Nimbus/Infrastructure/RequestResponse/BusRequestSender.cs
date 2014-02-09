using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class BusRequestSender : IRequestSender
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly ILogger _logger;
        private readonly IClock _clock;
        private readonly DefaultTimeoutSetting _responseTimeout;
        private readonly RequestTypesSetting _requestTypes;
        private readonly Lazy<HashSet<Type>> _validRequestTypes;

        internal BusRequestSender(INimbusMessagingFactory messagingFactory,
                                  ReplyQueueNameSetting replyQueueName,
                                  RequestResponseCorrelator requestResponseCorrelator,
                                  IClock clock,
                                  DefaultTimeoutSetting responseTimeout,
                                  RequestTypesSetting requestTypes,
                                  ILogger logger)
        {
            _messagingFactory = messagingFactory;
            _replyQueueName = replyQueueName;
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

            var correlationId = Guid.NewGuid();
            var requestTypeName = typeof (TRequest).FullName;
            var message = new BrokeredMessage(busRequest)
                          {
                              CorrelationId = correlationId.ToString(),
                              ReplyTo = _replyQueueName,
                              TimeToLive = timeout,
                          };
            message.Properties.Add(MessagePropertyKeys.MessageType, requestTypeName);

            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordRequest<TResponse>(correlationId, expiresAfter);

            var sender = _messagingFactory.GetQueueSender(PathFactory.QueuePathFor(busRequest.GetType()));

            _logger.Debug("Sending request message {0} of type {1}", correlationId, requestTypeName);
            await sender.Send(message);
            _logger.Debug("Sent request message {0} of type {1}", correlationId, requestTypeName);

            _logger.Debug("Waiting for response to request {0} of type {1}", correlationId, requestTypeName);
            var response = responseCorrelationWrapper.WaitForResponse(timeout);
            _logger.Debug("Received response to request {0} of type {1}", correlationId, requestTypeName);

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