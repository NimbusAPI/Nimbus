using System;

namespace Nimbus.Infrastructure
{
    internal interface IDeliveryRetryStrategy
    {
        DateTimeOffset CalculateNextRetryTime(NimbusMessage message);
    }
}