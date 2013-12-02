using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Timeouts
{
    internal class BusTimeoutSender : ITimeoutSender
    {
        private readonly IMessageSenderFactory _messageSenderFactory;
        private readonly IClock _clock;

        public BusTimeoutSender(IMessageSenderFactory messageSenderFactory, IClock clock)
        {
            _messageSenderFactory = messageSenderFactory;
            _clock = clock;
        }

        public async Task Defer<TBusTimeout>(TimeSpan delay, IBusTimeout busTimeout)
        {
            var sender = _messageSenderFactory.GetMessageSender(typeof(TBusTimeout));
            var message = new BrokeredMessage(busTimeout) { ScheduledEnqueueTimeUtc = _clock.UtcNow.Add(delay).DateTime };
            await sender.SendBatchAsync(new[] { message });
        }

        public async Task Defer<TBusTimeout>(DateTime processAt, IBusTimeout busTimeout)
        {
            var sender = _messageSenderFactory.GetMessageSender(typeof(TBusTimeout));
            var message = new BrokeredMessage(busTimeout) { ScheduledEnqueueTimeUtc = processAt };
            await sender.SendBatchAsync(new[] { message });
        }
    }
}