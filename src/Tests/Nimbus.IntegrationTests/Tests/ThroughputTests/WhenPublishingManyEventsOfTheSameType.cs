using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests
{
    [TestFixture]
    [Ignore("We pay $$ for messages when we're hitting the Azure Message Bus. Let's not run these on CI builds.")]
    public class WhenPublishingManyEventsOfTheSameType : ThroughputSpecificationForBus
    {
        protected override int ExpectedMessagesPerSecond
        {
            get { return 200; }
        }

        public override IEnumerable<Task> SendMessages(IBus bus)
        {
            for (var i = 0; i < NumMessagesToSend/2; i++)
            {
                yield return bus.Publish(new FooEvent());
                Console.Write(".");
            }
            Console.WriteLine();
        }
    }
}