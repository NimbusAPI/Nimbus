using System;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessagePumpFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly IClock _clock;
        private readonly ResponseMessagePumpDispatcher _dispatcher;
        private readonly IQueueManager _queueManager;

        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly ConcurrentHandlerLimitSetting _concurrentHandlerLimit;

        internal ResponseMessagePumpFactory(ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                            ReplyQueueNameSetting replyQueueName,
                                            IClock clock,
                                            ILogger logger,
                                            IQueueManager queueManager,
                                            ResponseMessagePumpDispatcher dispatcher)
        {
            _logger = logger;
            _queueManager = queueManager;
            _replyQueueName = replyQueueName;
            _clock = clock;
            _concurrentHandlerLimit = concurrentHandlerLimit;
            _dispatcher = dispatcher;
        }

        public IMessagePump Create()
        {
            var receiver = new NimbusQueueMessageReceiver(_queueManager, _replyQueueName, _concurrentHandlerLimit, _logger);
            _garbageMan.Add(receiver);

            var pump = new MessagePump(_clock, _logger, _dispatcher, receiver);
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