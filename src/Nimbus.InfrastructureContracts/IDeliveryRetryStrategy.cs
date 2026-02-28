using System;

namespace Nimbus.InfrastructureContracts
{
    public interface IDeliveryRetryStrategy
    {
        DateTimeOffset CalculateNextRetryTime(NimbusMessage message);
    }
}