using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Infrastructure;

namespace Nimbus.DevelopmentStubs
{
    internal class StubDeliveryRetryStrategy : IDeliveryRetryStrategy
    {
        public DateTimeOffset CalculateNextRetryTime(IEnumerable<DateTimeOffset> deliveryAttempts)
        {
            return deliveryAttempts.Last().AddSeconds(1);
        }
    }
}