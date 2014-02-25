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

        internal ResponseMessagePumpFactory(IQueueManager queueManager,
                                            ResponseMessagePumpDispatcher dispatcher,
                                            ILogger logger,
                                            ReplyQueueNameSetting replyQueueName,
                                            IClock clock)
        {
            _logger = logger;
            _queueManager = queueManager;
            _replyQueueName = replyQueueName;
            _clock = clock;
            _dispatcher = dispatcher;
        }

        public IMessagePump Create()
        {
            var receiver = new NimbusQueueMessageReceiver(_queueManager, _replyQueueName);
            _garbageMan.Add(receiver);

            var pump = new MessagePump(receiver, _dispatcher, _logger, _clock);
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