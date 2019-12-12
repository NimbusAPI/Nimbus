using System;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.Amqp.MessageSendersAndRecievers
{
    internal class AmqpMessageReciever : ThrottlingMessageReceiver
    {
        private readonly string _queuePath;
        private readonly ISerializer _serializer;

        public AmqpMessageReciever(string queuePath, ISerializer serializer,
            ConcurrentHandlerLimitSetting concurrentHandlerLimit, IGlobalHandlerThrottle globalHandlerThrottle,
            ILogger logger) : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queuePath = queuePath;
            _serializer = serializer;
        }

        protected override Task WarmUp()
        {
            return Task.CompletedTask;
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            var address = new Address("localhost:5672");
            var connection = new Connection(address);
            var session = new Session(connection);
            var receiver = new ReceiverLink(session, "receiver-drain", _queuePath);

            receiver.SetCredit(10);

            NimbusMessage message = null;
            var brokerMessage = await receiver.ReceiveAsync(TimeSpan.FromSeconds(300));
            if (brokerMessage != null)
            {
                receiver.Accept(brokerMessage);

                message =
                    (NimbusMessage) _serializer.Deserialize(brokerMessage.Body.ToString(), typeof(NimbusMessage));
            }

            receiver.Close();
            session.Close();
            connection.Close();

            return message;
        }
    }
}