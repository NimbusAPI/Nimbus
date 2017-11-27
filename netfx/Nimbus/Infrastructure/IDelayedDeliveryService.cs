using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    internal interface IDelayedDeliveryService
    {
        Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime);
    }
}