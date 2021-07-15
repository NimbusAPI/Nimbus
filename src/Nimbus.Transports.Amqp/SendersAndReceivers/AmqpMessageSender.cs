using System;
using System.Threading.Tasks;
using Amqp;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Amqp.ConnectionManagement;
using Nimbus.Transports.Amqp.Messages;

namespace Nimbus.Transports.Amqp.SendersAndReceivers
{
    public class AmqpMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IConnectionManager _connectionManager;
        private readonly string _queuePath;
        private readonly IMessageFactory _messageFactory;
        private ISenderLink _sender;

        internal AmqpMessageSender(IConnectionManager connectionManager, string queuePath, IMessageFactory messageFactory)
        {
            _connectionManager = connectionManager;
            _queuePath = queuePath;
            _messageFactory = messageFactory;
        }

        public async Task Send(NimbusMessage message)
        {
            var sender = GetSender();

            var brokerMessage = await _messageFactory.BuildMessage(message);
            await sender.SendAsync(brokerMessage);
            
        }

        private async void DiscardMessageSender()
        {
            await _sender.CloseAsync();
        }

        private ISenderLink GetSender()
        {
            if (_sender != null)
                return _sender;
            // var address = new Address("amqp://artemis:simetraehcapa@localhost:61616");
            // _connection = new Connection(address);
            // _session = new Session(_connection);
            // _sender = new SenderLink(_session, "test", _queuePath);
            _sender = _connectionManager.CreateMessageSender(_queuePath);
            return _sender;
        }

        public void Dispose()
        {
            DiscardMessageSender();
        }
    }
}