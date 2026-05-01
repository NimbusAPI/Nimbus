using System.Text;
using System.Threading.Tasks;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal class NatsQueueSender : INimbusMessageSender
    {
        private readonly string _queuePath;
        private readonly NatsConnectionFactory _connectionFactory;
        private readonly ISerializer _serializer;

        public NatsQueueSender(string queuePath, NatsConnectionFactory connectionFactory, ISerializer serializer)
        {
            _queuePath = queuePath;
            _connectionFactory = connectionFactory;
            _serializer = serializer;
        }

        public async Task Send(NimbusMessage message)
        {
            var serialized = _serializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(serialized);
            var connection = _connectionFactory.GetConnection();
            await connection.PublishAsync(_queuePath, bytes);
        }
    }
}
