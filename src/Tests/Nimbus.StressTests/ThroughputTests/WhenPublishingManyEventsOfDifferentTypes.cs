using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.MessageContracts;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenPublishingManyEventsOfDifferentTypes : ThroughputSpecificationForBus
    {
        public override async Task SendMessages(IBus bus)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < SendMessagesFor)
            {
                var messages = new IBusEvent[] {new FooEvent(), new BarEvent(), new BazEvent(), new QuxEvent()};
                await bus.PublishAll(messages);

                ExpectToReceiveMessages(8);
            }
        }
    }
}