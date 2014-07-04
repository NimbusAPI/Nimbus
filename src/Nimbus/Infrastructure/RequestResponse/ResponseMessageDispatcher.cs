using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessageDispatcher : IMessageDispatcher
    {
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;

        public ResponseMessageDispatcher(IBrokeredMessageFactory brokeredMessageFactory, RequestResponseCorrelator requestResponseCorrelator)
        {
            _requestResponseCorrelator = requestResponseCorrelator;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var requestId = Guid.Parse((string)message.Properties[MessagePropertyKeys.InReplyToRequestId]);
            var responseCorrelationWrapper = _requestResponseCorrelator.TryGetWrapper(requestId);
            if (responseCorrelationWrapper == null) return;

            var success = (bool) message.Properties[MessagePropertyKeys.RequestSuccessful];
            if (success)
            {
                var response = await _brokeredMessageFactory.GetBody(message);
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