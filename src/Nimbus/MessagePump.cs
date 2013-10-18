using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public class MessagePump<TMessage>
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IEventBroker _eventBroker;
        private MessageReceiver _reciever;

        public MessagePump(MessagingFactory messagingFactory, IEventBroker eventBroker)
        {
            _messagingFactory = messagingFactory;
            _eventBroker = eventBroker;
        }


        public void Start()
        {
            _reciever = _messagingFactory.CreateMessageReceiver("queue1");

            Task.Run(() => DoWork());
        }

        private void DoWork()
        {

            while (true)
            {
                var message = _reciever.Receive();

                _eventBroker.Publish(message.GetBody<TMessage>());
                message.Complete();
            }
        }
    }
}