using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class BusMulticastRequestSender : IMulticastRequestSender
    {
        private readonly TopicClientFactory _topicClientFactory;
        private readonly string _replyQueueName;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly IClock _clock;

        public BusMulticastRequestSender(TopicClientFactory topicClientFactory, string replyQueueName, RequestResponseCorrelator requestResponseCorrelator, IClock clock)
        {
            _topicClientFactory = topicClientFactory;
            _replyQueueName = replyQueueName;
            _requestResponseCorrelator = requestResponseCorrelator;
            _clock = clock;
        }

        public async Task<IEnumerable<TResponse>> SendRequest<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            var sender = _topicClientFactory.GetTopicClient(busRequest.GetType());

            var correlationId = Guid.NewGuid();
            var message = new BrokeredMessage(busRequest)
            {
                CorrelationId = correlationId.ToString(),
                ReplyTo = _replyQueueName,
            };

            message.Properties.Add(MessagePropertyKeys.RequestTimeoutInMillisecondsKey, (int) timeout.TotalMilliseconds);
            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordMulticastRequest<TResponse>(correlationId, expiresAfter);

            await sender.SendAsync(message);
            var response = responseCorrelationWrapper.WaitForResponses(timeout);

            return response;
        }
    }
}