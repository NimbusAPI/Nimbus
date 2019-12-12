using System.Threading;
using System.Threading.Tasks;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.InProcess.QueueManagement;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessQueueReceiver : ThrottlingMessageReceiver
    {
        private readonly string _queuePath;
        private readonly InProcessMessageStore _messageStore;

        private readonly ThreadSafeLazy<AsyncBlockingCollection<NimbusMessage>> _messageQueue;

        public InProcessQueueReceiver(string queuePath,
                                      ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                      InProcessMessageStore messageStore,
                                      IGlobalHandlerThrottle globalHandlerThrottle,
                                      ILogger logger) : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queuePath = queuePath;
            _messageStore = messageStore;

            _messageQueue = new ThreadSafeLazy<AsyncBlockingCollection<NimbusMessage>>(() => _messageStore.GetOrCreateMessageQueue(_queuePath));
        }

        public override string ToString()
        {
            return _queuePath;
        }

        protected override Task WarmUp()
        {
            return Task.Run(() => _messageQueue.EnsureValueCreated());
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            var nimbusMessage = await _messageQueue.Value.Take(cancellationToken);
            return nimbusMessage;
        }
    }
}