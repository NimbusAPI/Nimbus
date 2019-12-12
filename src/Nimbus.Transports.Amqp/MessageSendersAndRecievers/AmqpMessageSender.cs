using System.Threading.Tasks;
using Amqp;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Amqp.MessageSendersAndRecievers
{
    public class AmqpMessageSender : INimbusMessageSender
    {
        private readonly string _queuePath;
        private readonly ISerializer _serializer;

        public AmqpMessageSender(string queuePath, ISerializer serializer)
        {
            _queuePath = queuePath;
            _serializer = serializer;
        }

        public async Task Send(NimbusMessage message)
        {

            var address = new Address("amqp://artemis:simetraehcapa@localhost:61616");
            var connection = new Connection(address);
            var session = new Session(connection);
            var sender = new SenderLink(session, "test", _queuePath);

            var payload = _serializer.Serialize(message);
            var brokerMessage = new Message(payload);
            await sender.SendAsync(brokerMessage);
            
            
            await sender.CloseAsync();
            await session.CloseAsync();
            await connection.CloseAsync();


        }
    }
}