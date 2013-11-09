using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.Events
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
}