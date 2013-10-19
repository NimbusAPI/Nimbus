using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.MessagePumps
{
    public class ResponseMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly string _replyQueueName;
        private readonly RequestResponseCorrelator _requestResponseCorrelator;

        private MessageReceiver _receiver;

        public ResponseMessagePump(MessagingFactory messagingFactory, string replyQueueName, RequestResponseCorrelator requestResponseCorrelator)
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

        protected override void PumpMessage()
        {
            var message = _receiver.Receive(TimeSpan.FromSeconds(1));
            if (message == null) return;

            var correlationId = Guid.Parse(message.CorrelationId);
            var responseCorrelationWrapper = _requestResponseCorrelator.TryGetWrapper(correlationId);
            if (responseCorrelationWrapper == null) return;

            var responseType = responseCorrelationWrapper.ResponseType;
            var response = message.GetBody(responseType);

            responseCorrelationWrapper.SetResponse(response);
            message.Complete();
        }
    }
}