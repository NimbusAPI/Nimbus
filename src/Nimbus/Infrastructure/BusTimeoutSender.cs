using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    internal class BusTimeoutSender : ITimeoutSender
    {
        private readonly IMessageSenderFactory _messageSenderFactory;

        public BusTimeoutSender(IMessageSenderFactory messageSenderFactory)
        {
            _messageSenderFactory = messageSenderFactory;
        }

        public async Task Defer<TBusTimeout>(DateTime proccessAt, TBusTimeout busTimeout)
        {
            var sender = _messageSenderFactory.GetMessageSender(typeof(TBusTimeout));
            var message = new BrokeredMessage(busTimeout)
            {
                ScheduledEnqueueTimeUtc = proccessAt
            };
            await sender.SendBatchAsync(new[] { message });
        }

        public async Task Defer<TBusTimeout>(TimeSpan delay, TBusTimeout busTimeout)
        {
            await Defer<TBusTimeout>(DateTime.UtcNow + delay, busTimeout);
        }
    }
}