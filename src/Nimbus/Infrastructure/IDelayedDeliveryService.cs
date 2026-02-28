using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure
{
    internal interface IDelayedDeliveryService
    {
        Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime);
    }
}