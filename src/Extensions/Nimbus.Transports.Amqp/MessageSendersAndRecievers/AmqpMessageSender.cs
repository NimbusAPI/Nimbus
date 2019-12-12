using System.Threading.Tasks;
using Amqp;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.Amqp.MessageSendersAndRecievers
{
    public class AmqpMessageSender : INimbusMessageSender
    {
        private readonly string _queuePath;

        public AmqpMessageSender(string queuePath)
        {
            _queuePath = queuePath;
        }

        public async Task Send(NimbusMessage message)
        {

            var address = new Address("localhost:5672");
            var connection = new Connection(address);
            var session = new Session(connection);
            var sender = new SenderLink(session, "sender-spout", _queuePath);
            
            var brokerMessage = new Message(message);
            await sender.SendAsync(brokerMessage);
            
            sender.Close();
            session.Close();
            connection.Close();

        }
    }
}