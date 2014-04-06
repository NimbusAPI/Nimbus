using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests
{
    [TestFixture]
    [Explicit("We pay $$ for messages when we're hitting the Azure Message Bus. Let's not run these on CI builds.")]
    public class WhenSendingManyRequestsOfTheSameType : ThroughputSpecificationForBus
    {
        protected override int ExpectedMessagesPerSecond
        {
            get { return 200; }
        }

        protected override int NumMessagesToSend
        {
            get { return 1000; }
        }

        public override IEnumerable<Task> SendMessages(IBus bus)
        {
            for (var i = 0; i < NumMessagesToSend; i++)
            {
                yield return bus.Request(new FooRequest());
                Console.Write(".");
            }
            Console.WriteLine();
        }
    }
}