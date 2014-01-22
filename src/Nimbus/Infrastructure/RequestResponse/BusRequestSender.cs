using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class BusRequestSender : IRequestSender
    {
        private readonly IMessageSenderFactory _messageSenderFactory;
        private readonly string _replyQueueName;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly TimeSpan _responseTimeout;
        private readonly ILogger _logger;
        private readonly IClock _clock;
        private readonly HashSet<Type> _validRequestTypes;

        internal BusRequestSender(IMessageSenderFactory messageSenderFactory, string replyQueueName, RequestResponseCorrelator requestResponseCorrelator, IClock clock, TimeSpan responseTimeout, IReadOnlyList<Type> validRequestTypes, ILogger logger)
        {
            _messageSenderFactory = messageSenderFactory;
            _replyQueueName = replyQueueName;
            _requestResponseCorrelator = requestResponseCorrelator;
            _responseTimeout = responseTimeout;
            _logger = logger;
            _clock = clock;
            _validRequestTypes = new HashSet<Type>(validRequestTypes);
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
            var requestTypeName = typeof(TRequest).FullName;

            if (!_validRequestTypes.Contains(typeof(TRequest)))
                throw new BusException("The type {0} is not a recognised request type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(requestTypeName));

            var sender = _messageSenderFactory.GetMessageSender(busRequest.GetType());

            var correlationId = Guid.NewGuid();
            var message = new BrokeredMessage(busRequest)
            {
                CorrelationId = correlationId.ToString(),
                ReplyTo = _replyQueueName,
                TimeToLive = timeout,
            };
            message.Properties.Add(MessagePropertyKeys.MessageType, requestTypeName);

            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordRequest<TResponse>(correlationId, expiresAfter);

            _logger.Debug("Sending request message {0} of type {1}", correlationId, requestTypeName);
            await sender.SendAsync(message);
            _logger.Debug("Sent request message {0} of type {1}", correlationId, requestTypeName);

            _logger.Debug("Waiting for response to request {0} of type {1}", correlationId, requestTypeName);
            var response = responseCorrelationWrapper.WaitForResponse(timeout);
            _logger.Debug("Received response to request {0} of type {1}", correlationId, requestTypeName);

            return response;
        }
    }
}