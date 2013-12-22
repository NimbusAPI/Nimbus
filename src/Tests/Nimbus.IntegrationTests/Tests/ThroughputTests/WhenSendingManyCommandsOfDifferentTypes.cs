using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests
{
    [TestFixture]
    [Ignore("We pay $$ for messages when we're hitting the Azure Message Bus. Let's not run these on CI builds.")]
    public class WhenSendingManyCommandsOfDifferentTypes : ThroughputSpecificationForBus
    {
        protected override int ExpectedMessagesPerSecond
        {
            get { return 1000; }
        }

        public override IEnumerable<Task> SendMessages(IBus bus)
        {
            for (var i = 0; i < NumMessagesToSend/4; i++)
            {
                yield return bus.Send(new FooCommand());
                yield return bus.Send(new BarCommand());
                yield return bus.Send(new BazCommand());
                yield return bus.Send(new QuxCommand());
                Console.Write(".");
            }
            Console.WriteLine();
        }
    }
}