using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessagePumpFactory
    {
        private readonly ILogger _logger;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly DefaultBatchSizeSetting _defaultBatchSize;
        private readonly ResponseMessagePumpDispatcher _dispatcher;

        private readonly IQueueManager _queueManager;

        internal ResponseMessagePumpFactory(IQueueManager queueManager,
                                            ResponseMessagePumpDispatcher dispatcher,
                                            ILogger logger,
                                            ReplyQueueNameSetting replyQueueName,
                                            DefaultBatchSizeSetting defaultBatchSize)
        {
            _logger = logger;
            _queueManager = queueManager;
            _replyQueueName = replyQueueName;
            _defaultBatchSize = defaultBatchSize;
            _dispatcher = dispatcher;
        }

        public IMessagePump Create()
        {
            var receiver = new NimbusQueueMessageReceiver(_queueManager, _replyQueueName);
            var pump = new MessagePump(receiver, _dispatcher, _logger, _defaultBatchSize);
            return pump;
        }
    }
}