using System;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.DeliveryRetries
{
    public class SimpleBackoffDeliveryRetryStrategy : IDeliveryRetryStrategy
    {
        public DateTimeOffset CalculateNextRetryTime(NimbusMessage message)
        {
            var attemptCount = message.DeliveryAttempts.Length;
            var secondsToWait = Math.Pow(2, attemptCount + 1);
            return DateTimeOffset.Now.AddSeconds(secondsToWait);
        }
    }
}