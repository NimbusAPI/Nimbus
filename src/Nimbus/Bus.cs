using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public class Bus : IBus
    {
        private readonly string _connectionString;
        private readonly IEventBroker _eventBroker;
        private MessagingFactory _messagingFactory;

        public Bus(string connectionString, IEventBroker eventBroker)
        {
            _connectionString = connectionString;
            _eventBroker = eventBroker;
        }

        public void Send(object busCommand)
        {
            var message = new BrokeredMessage(busCommand);

            var sender = _messagingFactory.CreateMessageSender("queue1");
            sender.Send(message);

        }

        public void Start()
        {

            _messagingFactory = MessagingFactory.CreateFromConnectionString(_connectionString);

            var pump = new MessagePump<SomeCommand>(_messagingFactory, _eventBroker);
            pump.Start();
        }
    }
}