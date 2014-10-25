using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenSendingManyCommandsOfDifferentTypes : ThroughputSpecificationForBus
    {
        protected override int ExpectedMessagesPerSecond
        {
            get { return 150; }
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