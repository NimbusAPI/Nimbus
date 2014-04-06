using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.ThroughputTests.MessageContracts;
using Nimbus.MessageContracts;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests
{
    [TestFixture]
    [Explicit("We pay $$ for messages when we're hitting the Azure Message Bus. Let's not run these on CI builds.")]
    public class WhenPublishingManyEventsOfDifferentTypes : ThroughputSpecificationForBus
    {
        protected override int ExpectedMessagesPerSecond
        {
            get { return 1000; }
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