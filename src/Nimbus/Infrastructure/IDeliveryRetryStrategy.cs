using System;
using System.Collections.Generic;

namespace Nimbus.Infrastructure
{
    internal interface IDeliveryRetryStrategy
    {
        DateTimeOffset CalculateNextRetryTime(IEnumerable< DateTimeOffset> deliveryAttempts);
    }
}