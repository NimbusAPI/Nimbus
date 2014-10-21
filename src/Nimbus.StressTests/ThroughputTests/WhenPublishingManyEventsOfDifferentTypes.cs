using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.MessageContracts;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenPublishingManyEventsOfDifferentTypes : ThroughputSpecificationForBus
    {
        protected override int ExpectedMessagesPerSecond
        {
            get { return 400; }
        }

        public override IEnumerable<Task> SendMessages(IBus bus)
        {
            var messages = new List<IBusEvent>();
            for (var i = 0; i < NumMessagesToSend/8; i++) // /8 because we'll see each event once via multicast and once via competition
            {
                messages.Add(new FooEvent());
                messages.Add(new BarEvent());
                messages.Add(new BazEvent());
                messages.Add(new QuxEvent());
            }

            yield return bus.PublishAll(messages);
        }
    }
}