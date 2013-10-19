using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.MessagePumps
{
    public class BusRequestSender : IRequestSender
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly string _replyQueueName;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;

        public BusRequestSender(MessagingFactory messagingFactory, string replyQueueName, RequestResponseCorrelator requestResponseCorrelator)
        {
            _messagingFactory = messagingFactory;
            _replyQueueName = replyQueueName;
            _requestResponseCorrelator = requestResponseCorrelator;
        }

        public async Task<TResponse> SendRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
        {
            var queueName = busRequest.GetType().FullName;
            var sender = _messagingFactory.CreateMessageSender(queueName);

            var correlationId = Guid.NewGuid();
            var message = new BrokeredMessage(busRequest)
            {
                CorrelationId = correlationId.ToString(),
                ReplyTo = _replyQueueName,
            };

            var responseCorrelationWrapper = _requestResponseCorrelator.RecordRequest<TResponse>(correlationId);

            await sender.SendAsync(message);
            var response = responseCorrelationWrapper.WaitForResponse();

            return response;
        }
    }
}