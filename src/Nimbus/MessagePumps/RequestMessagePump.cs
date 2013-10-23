using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

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

        protected override BrokeredMessage[] ReceiveMessages()
        {
            return _reciever.ReceiveBatch(int.MaxValue, TimeSpan.FromSeconds(1)).ToArray();
        }

        protected override void PumpMessage(BrokeredMessage message)
        {
            Task.Run(() => HandleRequest(message));
        }

        private void HandleRequest(BrokeredMessage requestMessage)
        {
            var request = requestMessage.GetBody(_messageType);
            var response = _requestBroker.InvokeGenericHandleMethod(request);

            var replyQueueName = requestMessage.ReplyTo;
            var replyQueueClient = _messagingFactory.CreateQueueClient(replyQueueName);

            var responseMessage = new BrokeredMessage(response)
            {
                CorrelationId = requestMessage.CorrelationId,
            };

            replyQueueClient.Send(responseMessage);
        }
    }
}