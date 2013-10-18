using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.MessagePumps
{
    public class RequestMessagePump : IMessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IRequestBroker _requestBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;
        private bool _haveBeenToldToStop;


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
            _haveBeenToldToStop = true;
        }

        private void DoWork()
        {
            while (! _haveBeenToldToStop)
            {
                var requestMessage = _reciever.Receive(TimeSpan.FromSeconds(1));

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