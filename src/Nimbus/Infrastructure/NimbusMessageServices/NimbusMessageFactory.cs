using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Dispatching;

namespace Nimbus.Infrastructure.NimbusMessageServices
{
    internal class NimbusMessageFactory : INimbusMessageFactory
    {
        private readonly DefaultMessageTimeToLiveSetting _timeToLive;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly IClock _clock;
        private readonly IDispatchContextManager _dispatchContextManager;

        public NimbusMessageFactory(DefaultMessageTimeToLiveSetting timeToLive,
                                    ReplyQueueNameSetting replyQueueName,
                                    IClock clock,
                                    IDispatchContextManager dispatchContextManager)
        {
            _timeToLive = timeToLive;
            _replyQueueName = replyQueueName;
            _clock = clock;
            _dispatchContextManager = dispatchContextManager;
        }

        public Task<NimbusMessage> Create(object payload = null)
        {
            var nimbusMessage = new NimbusMessage(payload);
            var expiresAfter = _clock.UtcNow.AddSafely(_timeToLive.Value);
            var currentDispatchContext = _dispatchContextManager.GetCurrentDispatchContext();
            nimbusMessage.Properties[MessagePropertyKeys.PrecedingMessageId] = currentDispatchContext.ResultOfMessageId;
            nimbusMessage.CorrelationId = currentDispatchContext.CorrelationId;
            nimbusMessage.ReplyTo = _replyQueueName;
            nimbusMessage.ExpiresAfter = expiresAfter;
            nimbusMessage.Properties[MessagePropertyKeys.MessageType] = nimbusMessage.SafelyGetBodyTypeNameOrDefault();

            return Task.FromResult(nimbusMessage);
        }

        public Task<NimbusMessage> CreateSuccessfulResponse(object responsePayload, NimbusMessage originalRequest)
        {
            return Task.Run(async () =>
                                  {
                                      var responseMessage = (await Create(responsePayload)).WithReplyToRequestId(originalRequest.MessageId);
                                      responseMessage.Properties[MessagePropertyKeys.RequestSuccessful] = true;

                                      return responseMessage;
                                  });
        }

        public Task<NimbusMessage> CreateFailedResponse(NimbusMessage originalRequest, Exception exception)
        {
            return Task.Run(async () =>
                                  {
                                      var responseMessage = (await Create()).WithReplyToRequestId(originalRequest.MessageId);
                                      responseMessage.Properties[MessagePropertyKeys.RequestSuccessful] = false;
                                      foreach (var prop in exception.ExceptionDetailsAsProperties(_clock.UtcNow)) responseMessage.Properties.Add(prop.Key, prop.Value);

                                      return responseMessage;
                                  });
        }
    }
}