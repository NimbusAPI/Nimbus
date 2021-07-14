using System;
using System.Threading.Tasks;
using Amqp;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Amqp.SendersAndReceivers
{
    public class AmqpMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly string _queuePath;
        private readonly ISerializer _serializer;
        private Connection _connection;
        private Session _session;
        private SenderLink _sender;

        internal AmqpMessageSender(string queuePath, ISerializer serializer)
        {
            _queuePath = queuePath;
            _serializer = serializer;
        }

        public async Task Send(NimbusMessage message)
        {
            var sender = GetSender();

            var payload = _serializer.Serialize(message);
            var brokerMessage = new Message(payload);
            await sender.SendAsync(brokerMessage);
        }

        private async void DiscardMessageSender()
        {
            await _sender.CloseAsync();
            await _session.CloseAsync();
            await _connection.CloseAsync();
        }

        private SenderLink GetSender()
        {
            if (_sender != null)
                return _sender;

            var address = new Address("amqp://artemis:simetraehcapa@localhost:61616");
            _connection = new Connection(address);
            _session = new Session(_connection);
            _sender = new SenderLink(_session, "test", _queuePath);
            return _sender;
        }

        public void Dispose()
        {
            DiscardMessageSender();
        }
    }
}