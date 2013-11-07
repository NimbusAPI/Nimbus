using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Nimbus.IntegrationTests.ThroughputTests.MessageContracts;

namespace Nimbus.IntegrationTests.ThroughputTests
{
    [TestFixture]
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