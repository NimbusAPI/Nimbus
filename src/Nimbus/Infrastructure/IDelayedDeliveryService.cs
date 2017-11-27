using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    public interface IDelayedDeliveryService
    {
        Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime);
    }
}