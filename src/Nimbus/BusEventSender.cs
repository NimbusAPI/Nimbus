using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    internal class BusEventSender : IEventSender
    {
        private readonly ITopicClientFactory _topicClientFactory;

        public BusEventSender(ITopicClientFactory topicClientFactory)
        {
            _topicClientFactory = topicClientFactory;
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent)
        {
            var client = _topicClientFactory.GetTopicClient(typeof (TBusEvent));
            var brokeredMessage = new BrokeredMessage(busEvent);
            await client.SendBatchAsync(new[] {brokeredMessage});
        }
    }

    public interface ITopicClientFactory
    {
        TopicClient GetTopicClient(Type busEventType);
    }

    public class TopicClientFactory : ITopicClientFactory
    {
        private readonly MessagingFactory _messagingFactory;

        public TopicClientFactory(MessagingFactory messagingFactory)
        {
            _messagingFactory = messagingFactory;
        }

        public TopicClient GetTopicClient(Type busEventType)
        {
            return _messagingFactory.CreateTopicClient(busEventType.FullName);
        }
    }
}