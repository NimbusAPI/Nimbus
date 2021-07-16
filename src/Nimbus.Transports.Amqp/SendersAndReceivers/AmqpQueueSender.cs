using System;
using System.Threading.Tasks;
using Amqp;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Amqp.ConnectionManagement;
using Nimbus.Transports.Amqp.Messages;

namespace Nimbus.Transports.Amqp.SendersAndReceivers
{
    internal class AmqpQueueSender : INimbusMessageSender, IDisposable
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly IMessageFactory _messageFactory;
        private readonly string _queuePath;

        private ISenderLink _sender;

        internal AmqpQueueSender(
            IConnectionManager connectionManager,
            IMessageFactory messageFactory,
            ILogger logger,
            string queuePath)
        {
            _connectionManager = connectionManager;
            _queuePath = queuePath;
            _logger = logger;
            _messageFactory = messageFactory;
        }

        public async Task Send(NimbusMessage message)
        {
            var sender = GetSender();

            var brokerMessage = await _messageFactory.BuildMessage(message);
            await sender.SendAsync(brokerMessage);
        }

        public void Dispose()
        {
            Task.Run(async () =>
                     {
                         if (!_sender.IsClosed)
                         {
                             _logger.Debug("AMQP: Closing / Disposing of Sender {SenderName}.", _sender.Name);
                             await _sender.CloseAsync();
                             return;
                         }

                         _logger.Debug("AMQP: Sender {SenderName} already closed.", _sender.Name);
                     });
        }

        private ISenderLink GetSender()
        {
            if (_sender != null && !_sender.IsClosed)
                return _sender;

            _sender = _connectionManager.CreateQueueSender(_queuePath);
            return _sender;
        }
    }
}