using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;
using Nimbus.MessageContracts;

namespace Nimbus.MessagePumps
{
    public class BusRequestSender : IRequestSender
    {
        private readonly IMessageSenderFactory _messageSenderFactory;
        private readonly string _replyQueueName;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly TimeSpan _responseTimeout;

        public BusRequestSender(IMessageSenderFactory messageSenderFactory, string replyQueueName, RequestResponseCorrelator requestResponseCorrelator, TimeSpan responseTimeout)
        {
            _messageSenderFactory = messageSenderFactory;
            _replyQueueName = replyQueueName;
            _requestResponseCorrelator = requestResponseCorrelator;
            _responseTimeout = responseTimeout;
        }

        public async Task<TResponse> SendRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
        {
            var sender = _messageSenderFactory.GetMessageSender(busRequest.GetType());

            var correlationId = Guid.NewGuid();
            var message = new BrokeredMessage(busRequest)
            {
                CorrelationId = correlationId.ToString(),
                ReplyTo = _replyQueueName,
            };

            var responseCorrelationWrapper = _requestResponseCorrelator.RecordRequest<TResponse>(correlationId);

            await sender.SendAsync(message);
            var response = responseCorrelationWrapper.WaitForResponse(_responseTimeout);

            return response;
        }
    }
}