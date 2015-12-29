using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenSendingManyLargeCommandsOfTheSameType : ThroughputSpecificationForBus
    {
        public override async Task SendMessages(IBus bus)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < SendMessagesFor)
            {
                var command = new FooCommand
                              {
                                  SomeMessage = new string(Enumerable.Range(0, 32*1024).Select(j => '.').ToArray())
                              };
                await bus.Send(command);
                ExpectToReceiveMessages();
            }
        }
    }
}