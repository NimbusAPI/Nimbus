using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessageDispatcher : IMessageDispatcher
    {
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly ILogger _logger;

        public ResponseMessageDispatcher(ILogger logger, RequestResponseCorrelator requestResponseCorrelator)
        {
            _requestResponseCorrelator = requestResponseCorrelator;
            _logger = logger;
        }

        public async Task Dispatch(NimbusMessage message)
        {
            var requestId = (Guid)message.InReplyToMessageId;

            var responseCorrelationWrapper = _requestResponseCorrelator.TryGetWrapper(requestId);
            if (responseCorrelationWrapper == null)
            {
                _logger.Warn("Received a reply to request {MessageId} that had no corresponding request waiting for it.", requestId);
                return;
            }

            var success = (bool) message.Properties[MessagePropertyKeys.RequestSuccessful];
            if (success)
            {
                var response = message.Payload;
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