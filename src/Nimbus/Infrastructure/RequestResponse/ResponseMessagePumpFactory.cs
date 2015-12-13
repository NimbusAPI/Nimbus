using System;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessagePumpFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly ResponseMessageDispatcher _messageDispatcher;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly IQueueManager _queueManager;
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttemptSetting;
        private readonly IDeadLetterOffice _deadLetterOffice;
        private readonly IDelayedDeliveryService _delayedDeliveryService;
        private readonly IDeliveryRetryStrategy _deliveryRetryStrategy;

        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly ConcurrentHandlerLimitSetting _concurrentHandlerLimit;

        internal ResponseMessagePumpFactory(ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                            ReplyQueueNameSetting replyQueueName,
                                            IBrokeredMessageFactory brokeredMessageFactory,
                                            IClock clock,
                                            IDispatchContextManager dispatchContextManager,
                                            ILogger logger,
                                            IQueueManager queueManager,
                                            ResponseMessageDispatcher messageDispatcher,
                                            MaxDeliveryAttemptSetting maxDeliveryAttemptSetting,
                                            IDeadLetterOffice deadLetterOffice,
                                            IDelayedDeliveryService delayedDeliveryService,
                                            IDeliveryRetryStrategy deliveryRetryStrategy)
        {
            _concurrentHandlerLimit = concurrentHandlerLimit;
            _replyQueueName = replyQueueName;
            _clock = clock;
            _dispatchContextManager = dispatchContextManager;
            _logger = logger;
            _queueManager = queueManager;
            _messageDispatcher = messageDispatcher;
            _maxDeliveryAttemptSetting = maxDeliveryAttemptSetting;
            _deadLetterOffice = deadLetterOffice;
            _delayedDeliveryService = delayedDeliveryService;
            _deliveryRetryStrategy = deliveryRetryStrategy;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public IMessagePump Create()
        {
            var receiver = new NimbusQueueMessageReceiver(_brokeredMessageFactory, _queueManager, _replyQueueName, _concurrentHandlerLimit, _logger);
            _garbageMan.Add(receiver);

            var pump = new MessagePump(_maxDeliveryAttemptSetting,
                                       _clock,
                                       _dispatchContextManager,
                                       _logger,
                                       _messageDispatcher,
                                       receiver,
                                       _deadLetterOffice,
                                       _delayedDeliveryService,
                                       _deliveryRetryStrategy);
            _garbageMan.Add(pump);

            return pump;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _garbageMan.Dispose();
        }
    }
}