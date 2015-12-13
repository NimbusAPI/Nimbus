using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    internal interface IDelayedDeliveryService
    {
        Task DeliverAt(NimbusMessage message, DateTimeOffset deliveryTime);
    }
}