using System;
using Nimbus.Infrastructure;

namespace Nimbus.IntegrationTests.Stubs
{
    internal class ImmediateRetryDeliveryStrategy : IDeliveryRetryStrategy
    {
        public DateTimeOffset CalculateNextRetryTime(NimbusMessage message)
        {
            return DateTimeOffset.UtcNow;
        }
    }
}