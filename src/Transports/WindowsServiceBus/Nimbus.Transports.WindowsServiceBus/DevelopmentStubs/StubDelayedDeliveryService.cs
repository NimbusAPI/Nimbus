using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;

namespace Nimbus.Transports.WindowsServiceBus.DevelopmentStubs
{
    internal class StubDelayedDeliveryService : IDelayedDeliveryService
    {
        public Task DeliverAt(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            throw new NotImplementedException();
        }
    }
}