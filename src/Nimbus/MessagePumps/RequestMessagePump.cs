using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.MessagePumps
{
    public class RequestMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IRequestBroker _requestBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;

        public RequestMessagePump(MessagingFactory messagingFactory, IRequestBroker requestBroker, Type messageType)
        {
            _messagingFactory = messagingFactory;
            _requestBroker = requestBroker;
            _messageType = messageType;
        }

        public override void Start()
        {
            _reciever = _messagingFactory.CreateMessageReceiver(_messageType.FullName);
            base.Start();
        }

        public override void Stop()
        {
            if (_reciever != null) _reciever.Close();
            base.Stop();
        }

        protected override void PumpMessage()
        {
            var requestMessage = _reciever.Receive(TimeSpan.FromSeconds(1));
            if (requestMessage == null) return;

            var request = requestMessage.GetBody(_messageType);
            var response = _requestBroker.InvokeGenericHandleMethod(request);

            var replyQueueName = requestMessage.ReplyTo;
            var replyQueueClient = _messagingFactory.CreateQueueClient(replyQueueName);

            var responseMessage = new BrokeredMessage(response)
            {
                CorrelationId = requestMessage.CorrelationId,
            };

            replyQueueClient.Send(responseMessage);

            requestMessage.Complete();
        }
    }
}