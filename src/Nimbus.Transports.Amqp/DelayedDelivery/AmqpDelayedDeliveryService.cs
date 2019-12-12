using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Amqp.DelayedDelivery
{
    public class AmqpDelayedDeliveryService : IDelayedDeliveryService
    {
        public Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            throw new NotImplementedException();
        }
    }
}