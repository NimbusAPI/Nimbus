using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
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
        private readonly IClock _clock;
        private readonly HashSet<Type> _validRequestTypes;

        internal BusRequestSender(IMessageSenderFactory messageSenderFactory, string replyQueueName, RequestResponseCorrelator requestResponseCorrelator, IClock clock, TimeSpan responseTimeout, IReadOnlyList<Type> validRequestTypes)
        {
            _messageSenderFactory = messageSenderFactory;
            _replyQueueName = replyQueueName;
            _requestResponseCorrelator = requestResponseCorrelator;
            _responseTimeout = responseTimeout;
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
            if (!_validRequestTypes.Contains(typeof(TRequest)))
                throw new BusException("The type {0} is not a recognised request type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(typeof(TRequest).FullName));

            var sender = _messageSenderFactory.GetMessageSender(busRequest.GetType());

            var correlationId = Guid.NewGuid();
            var message = new BrokeredMessage(busRequest)
            {
                CorrelationId = correlationId.ToString(),
                ReplyTo = _replyQueueName,
            };
            message.Properties.Add(MessagePropertyKeys.MessageType, typeof(TRequest).FullName);

            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordRequest<TResponse>(correlationId, expiresAfter);

            await sender.SendAsync(message);
            var response = responseCorrelationWrapper.WaitForResponse(timeout);

            return response;
        }
    }
}