using System;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Amqp.ConnectionManagement;
using Nimbus.Transports.Amqp.Messages;

namespace Nimbus.Transports.Amqp.SendersAndReceivers
{
    internal class AmqpMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly IConnectionManager _connectionManager;
        private readonly string _queuePath;
        private readonly IMessageFactory _messageFactory;
        private IReceiverLink _receiver;

        public AmqpMessageReceiver(IConnectionManager connectionManager,
                                   string queuePath,
                                   IMessageFactory messageFactory,
                                   ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                   IGlobalHandlerThrottle globalHandlerThrottle,
                                   ILogger logger) : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _connectionManager = connectionManager;
            _queuePath = queuePath;
            _messageFactory = messageFactory;
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

                message = await _messageFactory.BuildNimbusMessage(brokerMessage);
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

        private IReceiverLink GetMessageReceiver()
        {
            if (_receiver != null) return _receiver;

            _receiver = _connectionManager.CreateMessageReceiver(_queuePath);

            return _receiver;
        }

        private async void DiscardMessageReceiver()
        {
            await _receiver.CloseAsync();
        }
    }
}