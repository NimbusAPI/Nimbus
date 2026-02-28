using System;
using Nimbus.InfrastructureContracts.Filtering;
using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Tests.Integration.Tests.SubscriptionFilterTests.EventHandlers
{
    public class MatchingSubscriptionFilter : ISubscriptionFilter
    {
        public static Guid MySpecialThingId = Guid.Parse("{9D57E250-05F8-4E42-9FFC-8874B60B605F}");

        public IFilterCondition FilterCondition => new MatchCondition("ThingId", MySpecialThingId);
    }
}