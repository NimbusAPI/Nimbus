using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.Commands
{
    internal class BusCommandSender : ICommandSender
    {
        private readonly IMessageSenderFactory _messageSenderFactory;
        private readonly IClock _clock;

        public BusCommandSender(IMessageSenderFactory messageSenderFactory, IClock clock)
        {
            _messageSenderFactory = messageSenderFactory;
            _clock = clock;
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            var sender = _messageSenderFactory.GetMessageSender(typeof (TBusCommand));
            var message = new BrokeredMessage(busCommand);
            await sender.SendBatchAsync(new[] {message});
        }

        public async Task SendAt<TBusCommand>(TimeSpan delay, TBusCommand busCommand)
        {
            await SendAt(_clock.UtcNow.Add(delay), busCommand);
        }

        public async Task SendAt<TBusCommand>(DateTimeOffset proccessAt, TBusCommand busCommand)
        {
            var sender = _messageSenderFactory.GetMessageSender(typeof(TBusCommand));
            var message = new BrokeredMessage(busCommand)
            {
                ScheduledEnqueueTimeUtc = proccessAt.DateTime
            };

            await sender.SendBatchAsync(new[] { message });
        }
    }
}