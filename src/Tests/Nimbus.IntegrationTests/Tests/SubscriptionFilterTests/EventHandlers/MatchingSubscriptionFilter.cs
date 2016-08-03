using System;
using Nimbus.Filtering;
using Nimbus.Filtering.Conditions;

namespace Nimbus.IntegrationTests.Tests.SubscriptionFilterTests.EventHandlers
{
    public class MatchingSubscriptionFilter : ISubscriptionFilter
    {
        public static Guid MySpecialThingId = Guid.Parse("{9D57E250-05F8-4E42-9FFC-8874B60B605F}");

        public IFilterCondition FilterCondition => new MatchCondition("ThingId", MySpecialThingId.ToString());
    }
}