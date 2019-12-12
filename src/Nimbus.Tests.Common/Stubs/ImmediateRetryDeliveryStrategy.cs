using System;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Tests.Common.Stubs
{
    public class ImmediateRetryDeliveryStrategy : IDeliveryRetryStrategy
    {
        public DateTimeOffset CalculateNextRetryTime(NimbusMessage message)
        {
            return DateTimeOffset.UtcNow;
        }
    }
}