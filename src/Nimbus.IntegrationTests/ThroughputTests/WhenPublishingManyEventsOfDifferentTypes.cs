using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Nimbus.IntegrationTests.ThroughputTests.MessageContracts;

namespace Nimbus.IntegrationTests.ThroughputTests
{
    [TestFixture]
    public class WhenPublishingManyEventsOfDifferentTypes : ThroughputSpecificationForBus
    {
        protected override int ExpectedMessagesPerSecond
        {
            get { return 1000; }
        }

        public override IEnumerable<Task> SendMessages(IBus bus)
        {
            for (var i = 0; i < NumMessagesToSend/8; i++)
            {
                yield return bus.Publish(new FooEvent());
                yield return bus.Publish(new BarEvent());
                yield return bus.Publish(new BazEvent());
                yield return bus.Publish(new QuxEvent());
                Console.Write(".");
            }
            Console.WriteLine();
        }
    }
}