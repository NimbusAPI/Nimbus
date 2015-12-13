using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;

namespace Nimbus.DevelopmentStubs
{
    internal class StubDelayedDeliveryService : IDelayedDeliveryService
    {
        public Task DeliverAt(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            throw new NotImplementedException();
        }
    }
}