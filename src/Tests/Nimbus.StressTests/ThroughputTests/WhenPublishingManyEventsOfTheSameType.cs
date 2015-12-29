using System.Diagnostics;
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
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < SendMessagesFor)
            {
                await bus.Publish(new FooEvent());
                ExpectToReceiveMessages(2);
            }
        }
    }
}