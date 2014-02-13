using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessagePumpDispatcher : IMessageDispatcher
    {
        private readonly RequestResponseCorrelator _requestResponseCorrelator;

        public ResponseMessagePumpDispatcher(RequestResponseCorrelator requestResponseCorrelator)
        {
            _requestResponseCorrelator = requestResponseCorrelator;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var correlationId = Guid.Parse(message.CorrelationId);
            var responseCorrelationWrapper = _requestResponseCorrelator.TryGetWrapper(correlationId);
            if (responseCorrelationWrapper == null) return;

            var success = (bool)message.Properties[MessagePropertyKeys.RequestSuccessful];
            if (success)
            {
                var responseType = responseCorrelationWrapper.ResponseType;
                var response = message.GetBody(responseType);
                responseCorrelationWrapper.Reply(response);
            }
            else
            {
                var exceptionMessage = (string)message.Properties[MessagePropertyKeys.ExceptionMessage];
                var exceptionStackTrace = (string)message.Properties[MessagePropertyKeys.ExceptionStackTrace];
                responseCorrelationWrapper.Throw(exceptionMessage, exceptionStackTrace);
            }
        }
    }
}