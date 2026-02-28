using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus
{
    public static class DelayedSendingExtensions
    {
        private static IClock _clock = new SystemClock();

        internal static void SetClockStrategy(IClock clock)
        {
            _clock = clock;
        }

        public static Task SendAfter<TBusCommand>(this IBus bus, TBusCommand busCommand, TimeSpan delay) where TBusCommand : IBusCommand
        {
            var deliveryTime = _clock.UtcNow.Add(delay);
            return bus.SendAt(busCommand, deliveryTime);
        }
    }
}