using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;

namespace Nimbus.Transports.Redis.DelayedDelivery
{
    internal class RedisDelayedDeliveryService : IDelayedDeliveryService
    {
        public Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            throw new NotImplementedException();
        }
    }
}