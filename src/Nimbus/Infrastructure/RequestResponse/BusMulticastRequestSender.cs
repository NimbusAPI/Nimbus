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
    internal class BusMulticastRequestSender : IMulticastRequestSender
    {
        private readonly TopicClientFactory _topicClientFactory;
        private readonly string _replyQueueName;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly IClock _clock;
        private readonly HashSet<Type> _validRequestTypes;

        public BusMulticastRequestSender(TopicClientFactory topicClientFactory,
                                         ReplyQueueNameSetting replyQueueName,
                                         RequestResponseCorrelator requestResponseCorrelator,
                                         IClock clock,
                                         RequestTypesSetting validRequestTypes)
        {
            _topicClientFactory = topicClientFactory;
            _replyQueueName = replyQueueName;
            _requestResponseCorrelator = requestResponseCorrelator;
            _clock = clock;
            _validRequestTypes = new HashSet<Type>(validRequestTypes.Value);
        }

        public async Task<IEnumerable<TResponse>> SendRequest<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            if (!_validRequestTypes.Contains(typeof (TRequest)))
                throw new BusException(
                    "The type {0} is not a recognised multicast request type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(
                        typeof (TRequest).FullName));

            var sender = _topicClientFactory.GetTopicClient(busRequest.GetType());

            var correlationId = Guid.NewGuid();
            var message = new BrokeredMessage(busRequest)
                          {
                              CorrelationId = correlationId.ToString(),
                              ReplyTo = _replyQueueName,
                          };
            message.Properties.Add(MessagePropertyKeys.MessageType, typeof(TRequest).FullName);
            message.Properties.Add(MessagePropertyKeys.RequestTimeoutInMillisecondsKey, (int) timeout.TotalMilliseconds);
            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordMulticastRequest<TResponse>(correlationId, expiresAfter);

            await sender.SendAsync(message);
            var response = responseCorrelationWrapper.WaitForResponses(timeout);

            return response;
        }
    }
}