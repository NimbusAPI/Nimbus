using System;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenPublishingManyEventsOfTheSameType : ThroughputSpecificationForBus
    {
        public override async Task SendMessages(IBus bus, Func<bool> shouldKeepSending)
        {
            while (shouldKeepSending())
            {
                await bus.Publish(new FooEvent());
                IncrementExpectedMessageCount(2);
            }
        }
    }
}