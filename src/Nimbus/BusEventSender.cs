using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    class BusEventSender : IEventSender
    {
        private readonly MessagingFactory _messagingFactory;

        public BusEventSender(MessagingFactory messagingFactory)
        {
            _messagingFactory = messagingFactory;
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent)
        {
            var client = _messagingFactory.CreateTopicClient(typeof(TBusEvent).FullName);
            var brokeredMessage = new BrokeredMessage(busEvent);
            await client.SendAsync(brokeredMessage);
        }
    }
}