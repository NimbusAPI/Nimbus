using System;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenPublishingManyEventsOfTheSameType : ThroughputSpecificationForBus
    {
        public override async Task SendMessages(IBus bus)
        {
            for (var i = 0; i < NumMessagesToSend/2; i++)
            {
                await bus.Publish(new FooEvent());
            }
            Console.WriteLine();
        }
    }
}