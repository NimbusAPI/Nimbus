using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessagePumpDispatcher : IMessageDispatcher
    {
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;

        public ResponseMessagePumpDispatcher(IBrokeredMessageFactory brokeredMessageFactory, RequestResponseCorrelator requestResponseCorrelator)
        {
            _requestResponseCorrelator = requestResponseCorrelator;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var correlationId = Guid.Parse(message.CorrelationId);
            var responseCorrelationWrapper = _requestResponseCorrelator.TryGetWrapper(correlationId);
            if (responseCorrelationWrapper == null) return;

            var success = (bool) message.Properties[MessagePropertyKeys.RequestSuccessful];
            if (success)
            {
                var responseType = responseCorrelationWrapper.ResponseType;
                var response = await _brokeredMessageFactory.GetBody(message, responseType);
                responseCorrelationWrapper.Reply(response);
            }
            else
            {
                var exceptionMessage = (string) message.Properties[MessagePropertyKeys.ExceptionMessage];
                var exceptionStackTrace = (string) message.Properties[MessagePropertyKeys.ExceptionStackTrace];
                responseCorrelationWrapper.Throw(exceptionMessage, exceptionStackTrace);
            }
        }
    }
}