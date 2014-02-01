using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class ResponseMessagePumpFactory
    {
        private readonly ILogger _logger;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly ResponseMessagePumpDispatcher _dispatcher;

        private readonly IQueueManager _queueManager;

        internal ResponseMessagePumpFactory(IQueueManager queueManager,
                                            ResponseMessagePumpDispatcher dispatcher,
                                            ILogger logger,
                                            ReplyQueueNameSetting replyQueueName)
        {
            _logger = logger;
            _queueManager = queueManager;
            _replyQueueName = replyQueueName;
            _dispatcher = dispatcher;
        }

        public IMessagePump Create()
        {
            var receiver = new NimbusQueueMessageReceiver(_queueManager, _replyQueueName);
            var pump = new MessagePump(receiver, _dispatcher, _logger);
            return pump;
        }
    }
}