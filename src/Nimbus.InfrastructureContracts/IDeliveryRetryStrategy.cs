using System;

namespace Nimbus.Infrastructure
{
    public interface IDeliveryRetryStrategy
    {
        DateTimeOffset CalculateNextRetryTime(NimbusMessage message);
    }
}