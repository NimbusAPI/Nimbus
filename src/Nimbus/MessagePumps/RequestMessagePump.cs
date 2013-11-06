using System;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.MessagePumps
{
    public class RequestMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IRequestBroker _requestBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;

        public RequestMessagePump(MessagingFactory messagingFactory, IRequestBroker requestBroker, Type messageType, ILogger logger) : base(logger)
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

        protected override BrokeredMessage[] ReceiveMessages()
        {
            return _reciever.ReceiveBatch(int.MaxValue, TimeSpan.FromSeconds(1)).ToArray();
        }

        protected override void PumpMessage(BrokeredMessage message)
        {
            HandleRequest(message);
        }

        private void HandleRequest(BrokeredMessage requestMessage)
        {
            var replyQueueName = requestMessage.ReplyTo;
            var replyQueueClient = _messagingFactory.CreateQueueClient(replyQueueName);

            var request = requestMessage.GetBody(_messageType);

            BrokeredMessage responseMessage;
            try
            {
                var response = _requestBroker.InvokeGenericHandleMethod(request);
                responseMessage = new BrokeredMessage(response);
                responseMessage.Properties.Add(MessagePropertyKeys.RequestSuccessfulKey, true);
            }
            catch (Exception exc)
            {
                responseMessage = new BrokeredMessage();
                responseMessage.Properties.Add(MessagePropertyKeys.RequestSuccessfulKey, false);
                foreach (var prop in ExceptionDetailsAsProperties(exc)) responseMessage.Properties.Add(prop.Key, prop.Value);
            }

            responseMessage.CorrelationId = requestMessage.CorrelationId;
            replyQueueClient.Send(responseMessage);
        }
    }
}