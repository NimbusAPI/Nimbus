using System;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Amqp.SendersAndReceivers
{
    internal class AmqpMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly string _queuePath;
        private readonly ISerializer _serializer;
        private ReceiverLink _receiver;
        private Connection _connection;
        private Session _session;

        public AmqpMessageReceiver(string queuePath, ISerializer serializer,
            ConcurrentHandlerLimitSetting concurrentHandlerLimit, IGlobalHandlerThrottle globalHandlerThrottle,
            ILogger logger) : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queuePath = queuePath;
            _serializer = serializer;
        }

        protected override Task WarmUp()
        {
            GetMessageReceiver();
            return Task.CompletedTask;
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {

            var receiver = GetMessageReceiver();
            NimbusMessage message = null;
            var brokerMessage = await receiver.ReceiveAsync(TimeSpan.FromSeconds(300));
            if (brokerMessage != null)
            {
                receiver.Accept(brokerMessage);

                message =
                    (NimbusMessage) _serializer.Deserialize(brokerMessage.Body.ToString(), typeof(NimbusMessage));
            }

            return message;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing) return;

                DiscardMessageReceiver();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private ReceiverLink GetMessageReceiver()
        {
            if (_receiver != null) return _receiver;

            var address = new Address("amqp://artemis:simetraehcapa@localhost:61616");
            _connection = new Connection(address);
            _session = new Session(_connection);
            _receiver = new ReceiverLink(_session, "test", _queuePath);
            _receiver.SetCredit(10);

            return _receiver;
        }

        private async void DiscardMessageReceiver()
        {
            await _receiver.CloseAsync();
            await _session.CloseAsync();
            await _connection.CloseAsync();
        }
    }
}