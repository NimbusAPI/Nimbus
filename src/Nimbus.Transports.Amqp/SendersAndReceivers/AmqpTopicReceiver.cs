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
    internal class AmqpTopicReceiver : ThrottlingMessageReceiver
    {
        private readonly IConnectionManager _connectionManager;
        private readonly string _topicPath;
        private readonly IMessageFactory _messageFactory;

        private IReceiverLink _receiver;

        public AmqpTopicReceiver(
            IConnectionManager connectionManager, 
            IMessageFactory messageFactory, 
            string topicPath,
            ConcurrentHandlerLimitSetting concurrentHandlerLimit,
            IGlobalHandlerThrottle globalHandlerThrottle,
            ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _connectionManager = connectionManager;
            _messageFactory = messageFactory;
            _topicPath = topicPath;
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
        
        private IReceiverLink GetMessageReceiver()
        {
            if (_receiver != null && !_receiver.IsClosed)
                return _receiver;

            _receiver = _connectionManager.CreateTopicReceiver(_topicPath);

            return _receiver;
        }

    }
}