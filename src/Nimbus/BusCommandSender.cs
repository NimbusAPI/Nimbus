using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    class BusCommandSender : ICommandSender
    {
        private readonly MessagingFactory _messagingFactory;

        public BusCommandSender(MessagingFactory messagingFactory)
        {
            _messagingFactory = messagingFactory;
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            var sender = _messagingFactory.CreateMessageSender(typeof(TBusCommand).FullName);
            var message = new BrokeredMessage(busCommand);
            await sender.SendAsync(message);
        }
    }
}