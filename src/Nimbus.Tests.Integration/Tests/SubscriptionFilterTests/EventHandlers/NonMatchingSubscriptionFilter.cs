using System;
using Nimbus.InfrastructureContracts.Filtering;
using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Tests.Integration.Tests.SubscriptionFilterTests.EventHandlers
{
    public class NonMatchingSubscriptionFilter : ISubscriptionFilter
    {
        public static Guid MySpecialThingId = Guid.Parse("{7F1323F0-5983-414D-91C2-94B24DE2C581}");

        public IFilterCondition FilterCondition => new MatchCondition("ThingId", MySpecialThingId);
    }
}