using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessageDispatcher : IMessageDispatcher
    {
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly INimbusMessageFactory _nimbusMessageFactory;

        public ResponseMessageDispatcher(INimbusMessageFactory nimbusMessageFactory, RequestResponseCorrelator requestResponseCorrelator)
        {
            _requestResponseCorrelator = requestResponseCorrelator;
            _nimbusMessageFactory = nimbusMessageFactory;
        }

        public async Task Dispatch(NimbusMessage message)
        {
            var requestId = Guid.Parse((string)message.Properties[MessagePropertyKeys.InReplyToRequestId]);
            var responseCorrelationWrapper = _requestResponseCorrelator.TryGetWrapper(requestId);
            if (responseCorrelationWrapper == null) return;

            var success = (bool) message.Properties[MessagePropertyKeys.RequestSuccessful];
            if (success)
            {
                var response = await _nimbusMessageFactory.GetBody(message);
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