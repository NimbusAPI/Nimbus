using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.Commands
{
    internal class BusCommandSender : ICommandSender
    {
        private readonly IMessageSenderFactory _messageSenderFactory;

        public BusCommandSender(IMessageSenderFactory messageSenderFactory)
        {
            _messageSenderFactory = messageSenderFactory;
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            var sender = _messageSenderFactory.GetMessageSender(typeof (TBusCommand));
            var message = new BrokeredMessage(busCommand);
            await sender.SendBatchAsync(new[] {message});
        }
    }
}