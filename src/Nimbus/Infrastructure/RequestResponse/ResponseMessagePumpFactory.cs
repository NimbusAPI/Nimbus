using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessagePumpFactory
    {
        private readonly ILogger _logger;
        private readonly PoorMansIoC _container;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly ResponseMessageDispatcher _responseMessageDispatcher;

        private readonly ConcurrentHandlerLimitSetting _concurrentHandlerLimit;

        internal ResponseMessagePumpFactory(ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                            ReplyQueueNameSetting replyQueueName,
                                            IBrokeredMessageFactory brokeredMessageFactory,
                                            ILogger logger,
                                            IQueueManager queueManager,
                                            PoorMansIoC container,
                                            ResponseMessageDispatcher responseMessageDispatcher)
        {
            _concurrentHandlerLimit = concurrentHandlerLimit;
            _replyQueueName = replyQueueName;
            _logger = logger;
            _queueManager = queueManager;
            _responseMessageDispatcher = responseMessageDispatcher;
            _container = container;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public IMessagePump Create()
        {
            var receiver = new NimbusQueueMessageReceiver(_brokeredMessageFactory, _queueManager, _replyQueueName, _concurrentHandlerLimit, _logger);
            var pump = _container.ResolveWithOverrides<MessagePump>(receiver, _responseMessageDispatcher);
            return pump;
        }
    }
}