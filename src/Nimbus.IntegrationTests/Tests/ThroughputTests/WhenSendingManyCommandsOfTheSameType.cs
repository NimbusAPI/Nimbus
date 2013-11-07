using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Nimbus.IntegrationTests.Tests.ThroughputTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests
{
    [TestFixture]
    [Ignore("We pay $$ for messages when we're hitting the Azure Message Bus. Let's not run these on CI builds.")]
    public class WhenSendingManyCommandsOfTheSameType : ThroughputSpecificationForBus
    {
        protected override int ExpectedMessagesPerSecond
        {
            get { return 200; }
        }

        public override IEnumerable<Task> SendMessages(IBus bus)
        {
            for (var i = 0; i < NumMessagesToSend; i++)
            {
                yield return bus.Send(new FooCommand());
                Console.Write(".");
            }
            Console.WriteLine();
        }
    }
}