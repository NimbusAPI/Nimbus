using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using NullGuard;

namespace Nimbus.Infrastructure
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

        public Task<NimbusMessage> Create(string destinationPath,  [AllowNull] object payload)
        {
            var nimbusMessage = new NimbusMessage(destinationPath, payload);
            var expiresAfter = _clock.UtcNow.AddSafely(_timeToLive.Value);
            var currentDispatchContext = _dispatchContextManager.GetCurrentDispatchContext();
            nimbusMessage.PrecedingMessageId = currentDispatchContext.ResultOfMessageId;
            nimbusMessage.CorrelationId = currentDispatchContext.CorrelationId;
            nimbusMessage.From = _replyQueueName;
            nimbusMessage.ExpiresAfter = expiresAfter;

            payload?.GetType().GetProperties()
                    .Where(p => p.HasAttribute<FilterProperty>())
                    .Do(p => nimbusMessage.Properties[p.Name] = p.GetValue(payload))
                    .Done();

            return Task.FromResult(nimbusMessage);
        }

        public async Task<NimbusMessage> CreateSuccessfulResponse(string destinationPath, object responsePayload, NimbusMessage originalRequest)
        {
            var responseMessage = (await Create(destinationPath, responsePayload))
                .WithReplyToRequestId(originalRequest.MessageId)
                .WithProperty(MessagePropertyKeys.RequestSuccessful, true);

            return responseMessage;
        }

        public async Task<NimbusMessage> CreateFailedResponse(string destinationPath, NimbusMessage originalRequest, Exception exception)
        {
            var responseMessage = (await Create(destinationPath, null))
                .WithReplyToRequestId(originalRequest.MessageId)
                .WithProperty(MessagePropertyKeys.RequestSuccessful, false);

            foreach (var prop in exception.ExceptionDetailsAsProperties(_clock.UtcNow)) responseMessage.Properties.Add(prop.Key, prop.Value);

            return responseMessage;
        }
    }
}