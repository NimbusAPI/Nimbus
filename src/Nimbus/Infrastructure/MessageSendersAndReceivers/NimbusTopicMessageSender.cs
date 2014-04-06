using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusTopicMessageSender : INimbusMessageSender
    {
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;

        private readonly Lazy<TopicClient> _topicClient;
        readonly SemaphoreSlim _throttle = new SemaphoreSlim(10);

        public NimbusTopicMessageSender(IQueueManager queueManager, string topicPath)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;

            _topicClient = new Lazy<TopicClient>(() => _queueManager.CreateTopicSender(_topicPath));
        }

        public async Task Send(BrokeredMessage message)
        {
            _throttle.Wait();
            try
            {
                await _topicClient.Value.SendAsync(message);
            }
            finally
            {
                _throttle.Release();
            }
        }

        public void Dispose()
        {
            if (!_topicClient.IsValueCreated) return;
            _topicClient.Value.Close();
        }
    }
}