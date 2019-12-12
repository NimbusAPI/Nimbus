using System;
using System.Linq;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.DevelopmentStubs
{
    internal class StubDeliveryRetryStrategy : IDeliveryRetryStrategy
    {
        public DateTimeOffset CalculateNextRetryTime(NimbusMessage message)
        {
            return message.DeliveryAttempts.Max().AddSeconds(1);
        }
    }
}