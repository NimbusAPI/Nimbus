using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class BusRequestSender : IRequestSender
    {
        private readonly IMessageSenderFactory _messageSenderFactory;
        private readonly string _replyQueueName;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly TimeSpan _responseTimeout;
        private readonly IClock _clock;

        internal BusRequestSender(IMessageSenderFactory messageSenderFactory,
                                  string replyQueueName,
                                  RequestResponseCorrelator requestResponseCorrelator,
                                  IClock clock,
                                  TimeSpan responseTimeout)
        {
            _messageSenderFactory = messageSenderFactory;
            _replyQueueName = replyQueueName;
            _requestResponseCorrelator = requestResponseCorrelator;
            _responseTimeout = responseTimeout;
            _clock = clock;
        }

        public async Task<TResponse> SendRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            return await SendRequest(busRequest, _responseTimeout);
        }

        public async Task<TResponse> SendRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            var sender = _messageSenderFactory.GetMessageSender(busRequest.GetType());

            var correlationId = Guid.NewGuid();
            var message = new BrokeredMessage(busRequest)
            {
                CorrelationId = correlationId.ToString(),
                ReplyTo = _replyQueueName,
            };

            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordRequest<TResponse>(correlationId, expiresAfter);

            await sender.SendAsync(message);
            var response = responseCorrelationWrapper.WaitForResponse(timeout);

            return response;
        }
    }
}