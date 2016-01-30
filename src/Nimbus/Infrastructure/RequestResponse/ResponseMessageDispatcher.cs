using System;
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
            if (!message.InReplyToMessageId.HasValue)
            {
                _logger.Error($"Received a reply message without an {nameof(NimbusMessage.InReplyToMessageId)} property.");
                return;
            }

            var requestId = (Guid) message.InReplyToMessageId;

            var responseCorrelationWrapper = _requestResponseCorrelator.TryGetWrapper(requestId);
            if (responseCorrelationWrapper == null)
            {
                _logger.Warn("Received a reply to request {RequestMessageId} that had no corresponding request waiting for it.", requestId);
                return;
            }

            var success = message.Properties[MessagePropertyKeys.RequestSuccessful] as bool? ?? false;
            if (success)
            {
                _logger.Debug("Received successful response");

                var response = message.Payload;
                responseCorrelationWrapper.Reply(response);
            }
            else
            {
                _logger.Debug("Received failure response");

                var exceptionMessage = (string) message.Properties[MessagePropertyKeys.ExceptionMessage];
                var exceptionStackTrace = (string) message.Properties[MessagePropertyKeys.ExceptionStackTrace];
                responseCorrelationWrapper.Throw(exceptionMessage, exceptionStackTrace);
            }
        }
    }
}