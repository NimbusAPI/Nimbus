using System.Threading;
using System.Threading.Tasks;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessQueueReceiver : ThrottlingMessageReceiver
    {
        private readonly string _queuePath;
        private readonly AsyncBlockingCollection<NimbusMessage> _messageQueue;

        public InProcessQueueReceiver(string queuePath,
                                      AsyncBlockingCollection<NimbusMessage> messageQueue,
                                      ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                      IGlobalHandlerThrottle globalHandlerThrottle,
                                      ILogger logger) : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queuePath = queuePath;
            _messageQueue = messageQueue;
        }

        public override string ToString()
        {
            return _queuePath;
        }

        protected override Task WarmUp()
        {
            return Task.Delay(0);
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            var nimbusMessage = await _messageQueue.Take(cancellationToken);
            return nimbusMessage;
        }
    }
}