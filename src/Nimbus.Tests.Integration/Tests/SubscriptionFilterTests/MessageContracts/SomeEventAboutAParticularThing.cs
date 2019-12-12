using System;
using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.SubscriptionFilterTests.MessageContracts
{
    public class SomeEventAboutAParticularThing: IBusEvent
    {
        [FilterProperty]
        public Guid ThingId { get; set; }
    }
}