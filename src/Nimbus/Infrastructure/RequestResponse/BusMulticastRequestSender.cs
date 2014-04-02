using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class BusMulticastRequestSender : IMulticastRequestSender
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly HashSet<Type> _validRequestTypes;

        public BusMulticastRequestSender(INimbusMessagingFactory messagingFactory,
                                         IBrokeredMessageFactory brokeredMessageFactory,
                                         RequestResponseCorrelator requestResponseCorrelator,
                                         IClock clock,
                                         RequestTypesSetting validRequestTypes,
                                         ILogger logger)
        {
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _requestResponseCorrelator = requestResponseCorrelator;
            _clock = clock;
            _logger = logger;
            _validRequestTypes = new HashSet<Type>(validRequestTypes.Value);
        }

        public async Task<IEnumerable<TResponse>> SendRequest<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            AssertValidRequestType<TRequest, TResponse>();

            var message = _brokeredMessageFactory.Create(busRequest).WithRequestTimeout(timeout);
            var expiresAfter = _clock.UtcNow.Add(timeout);
            var responseCorrelationWrapper = _requestResponseCorrelator.RecordMulticastRequest<TResponse>(Guid.Parse(message.CorrelationId), expiresAfter);

            var topicPath = PathFactory.TopicPathFor(busRequest.GetType());
            var sender = _messagingFactory.GetTopicSender(topicPath);
            
            LogActivity("Sending multicast request", message, topicPath);
            await sender.Send(message);
            LogActivity("Sent multicast request", message, topicPath);

            _logger.Debug("Waiting for multicast response to {0} from {1} [MessageId:{2}, CorrelationId:{3}]",
                message.SafelyGetBodyTypeNameOrDefault(), topicPath, message.MessageId, message.CorrelationId);
            var response = responseCorrelationWrapper.ReturnResponsesOpportunistically(timeout);
            _logger.Debug("Received response to {0} from {1} [MessageId:{2}, CorrelationId:{3}] in the form of {4}",
                message.SafelyGetBodyTypeNameOrDefault(), topicPath, message.MessageId, message.CorrelationId, response.GetType().FullName);

            return response;
        }

        private void LogActivity(string activity, BrokeredMessage message, string path)
        {
            _logger.Debug("{0} {1} to {2} [MessageId:{3}, CorrelationId:{4}]",
                activity, message.SafelyGetBodyTypeNameOrDefault(), path, message.MessageId, message.CorrelationId);
        }
        
        private void AssertValidRequestType<TRequest, TResponse>()
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            if (!_validRequestTypes.Contains(typeof (TRequest)))
                throw new BusException(
                    "The type {0} is not a recognised multicast request type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(
                        typeof (TRequest).FullName));
        }
    }
}