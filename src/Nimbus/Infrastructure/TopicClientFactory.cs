using System;
using System.Collections.Concurrent;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public class TopicClientFactory : ITopicClientFactory, IDisposable
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly ConcurrentDictionary<Type, TopicClient> _topicClients = new ConcurrentDictionary<Type, TopicClient>();

        public TopicClientFactory(MessagingFactory messagingFactory)
        {
            _messagingFactory = messagingFactory;
        }

        public TopicClient GetTopicClient(Type busEventType)
        {
            return _topicClients.GetOrAdd(busEventType, t => _messagingFactory.CreateTopicClient(PathFactory.TopicPathFor(t)));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            foreach (var topicClient in _topicClients.Values)
            {
                topicClient.Close();
            }
        }
    }
}