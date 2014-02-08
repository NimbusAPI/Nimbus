using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusTopicMessageSender : INimbusMessageSender
    {
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;

        private readonly Lazy<TopicClient> _topicClient;

        public NimbusTopicMessageSender(IQueueManager queueManager, string topicPath)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;

            _topicClient = new Lazy<TopicClient>(() => _queueManager.CreateTopicSender(_topicPath));
        }

        public async Task Send(BrokeredMessage message)
        {
            await _topicClient.Value.SendAsync(message);
        }
    }
}