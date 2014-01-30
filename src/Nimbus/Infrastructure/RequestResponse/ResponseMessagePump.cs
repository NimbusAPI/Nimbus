using System;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class ResponseMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly string _replyQueueName;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;

        private MessageReceiver _receiver;

        internal ResponseMessagePump(MessagingFactory messagingFactory, string replyQueueName, RequestResponseCorrelator requestResponseCorrelator, ILogger logger, int batchSize)
            : base(logger, batchSize)
        {
            _messagingFactory = messagingFactory;
            _replyQueueName = replyQueueName;
            _requestResponseCorrelator = requestResponseCorrelator;
        }

        public override void Start()
        {
            _receiver = _messagingFactory.CreateMessageReceiver(_replyQueueName);

            base.Start();
        }

        public override void Stop()
        {
            if (_receiver != null) _receiver.Close();

            base.Stop();
        }

        protected override BrokeredMessage[] ReceiveMessages()
        {
            return _receiver.ReceiveBatch(BatchSize, BatchTimeout).ToArray();
        }

        protected override void PumpMessage(BrokeredMessage message)
        {
            var correlationId = Guid.Parse(message.CorrelationId);
            var responseCorrelationWrapper = _requestResponseCorrelator.TryGetWrapper(correlationId);
            if (responseCorrelationWrapper == null)
            {
                Logger.Debug("Could not find correlation wrapper for reply {0} ({1}", correlationId, message.Properties[MessagePropertyKeys.MessageType]);
                return;
            }

            var success = (bool) message.Properties[MessagePropertyKeys.RequestSuccessfulKey];
            if (success)
            {
                Logger.Debug("Request {0} was successful. Dispatching reply to correlation wrapper.", correlationId);

                var responseType = responseCorrelationWrapper.ResponseType;
                var response = message.GetBody(responseType);
                responseCorrelationWrapper.Reply(response);

                Logger.Debug("Response {0} dispatched.", correlationId);
            }
            else
            {
                var exceptionMessage = (string) message.Properties[MessagePropertyKeys.ExceptionMessageKey];
                var exceptionStackTrace = (string) message.Properties[MessagePropertyKeys.ExceptionStackTraceKey];

                Logger.Debug("Request {0} failed. Dispatching exception to correlation wrapper: {1} {2}", correlationId, exceptionMessage, exceptionStackTrace);

                responseCorrelationWrapper.Throw(exceptionMessage, exceptionStackTrace);
            }
        }
    }
}