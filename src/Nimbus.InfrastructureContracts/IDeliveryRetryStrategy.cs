using System;

namespace Nimbus
{
    public interface IDeliveryRetryStrategy
    {
        DateTimeOffset CalculateNextRetryTime(NimbusMessage message);
    }
}