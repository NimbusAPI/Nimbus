using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenSendingManyLargeCommandsOfTheSameType : ThroughputSpecificationForBus
    {
        public override async Task SendMessages(IBus bus, Func<bool> shouldKeepSending)
        {
            while (shouldKeepSending())
            {
                var command = new FooCommand
                              {
                                  SomeMessage = new string(Enumerable.Range(0, 32*1024).Select(j => '.').ToArray())
                              };
                await bus.Send(command);
                IncrementExpectedMessageCount();
            }
        }
    }
}