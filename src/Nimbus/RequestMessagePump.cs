using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public class RequestMessagePump : IMessagePump
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

        public void Start()
        {
            _reciever = _messagingFactory.CreateMessageReceiver(_messageType.FullName);

            Task.Run(() => DoWork());
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        private void DoWork()
        {
            while (true)
            {
                var requestMessage = _reciever.Receive();

                var request = requestMessage.GetBody(_messageType);
                var response = _requestBroker.HandleAwful(request);

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
}