using System;
using System.Linq;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.DeliveryRetries
{
    internal class NoWaitDeliveryRetryStrategy : IDeliveryRetryStrategy
    {
        public DateTimeOffset CalculateNextRetryTime(NimbusMessage message)
        {
            return message.DeliveryAttempts.Max().AddSeconds(1);
        }
    }
}