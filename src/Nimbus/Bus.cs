using System;
using System.Collections.Generic;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public class Bus : IBus
    {
        private readonly string _connectionString;
        private readonly IEventBroker _eventBroker;
        private readonly Type[] _commandTypes;
        private MessagingFactory _messagingFactory;
        private readonly IList<IMessagePump> _messagePumps = new List<IMessagePump>();

        public Bus(string connectionString, IEventBroker eventBroker, Type[] commandTypes)
        {
            _connectionString = connectionString;
            _eventBroker = eventBroker;
            _commandTypes = commandTypes;
        }

        public void Send(object busCommand)
        {
            var sender = _messagingFactory.CreateMessageSender("queue1");
            for (var i = 0; i < 100; i++)
            {
                var message = new BrokeredMessage(busCommand);
                sender.Send(message);
            }
        }

        public void Start()
        {
            _messagingFactory = MessagingFactory.CreateFromConnectionString(_connectionString);

            foreach (var commandType in _commandTypes)
            {
                var pump = new MessagePump(_messagingFactory, _eventBroker, commandType);
                _messagePumps.Add(pump);
                pump.Start();
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}