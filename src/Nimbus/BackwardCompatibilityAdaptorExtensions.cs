using System;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus
{
    public static class BackwardCompatibilityAdaptorExtensions
    {
        [Obsolete("Deprecated in favour of .SendAt(...) and .SendAfter(...) methods. This adaptor method will be removed in a future release.")]
        public static Task Defer<TBusCommand>(this IBus bus, TimeSpan delay, TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            var deliveryTime = DateTimeOffset.UtcNow.Add(delay);
            return bus.SendAt(busCommand, deliveryTime);
        }

        [Obsolete("Deprecated in favour of .SendAt(...) and .SendAfter(...) methods. This adaptor method will be removed in a future release.")]
        public static Task Defer<TBusCommand>(this IBus bus, DateTimeOffset processAt, TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            return bus.SendAt(busCommand, processAt);
        }
    }
}