using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    public class WhenPublishingAnEventThatIsNotHandled : TestForBus
    {
        public override async Task When()
        {
            var myEvent = new SomeEventWeDoNotHandle();
            await Bus.Publish(myEvent);
        }

        [Test]
        public async Task NoExceptionIsThrown()
        {
            await Given();
            Should.NotThrow(async () => await When());
        }
    }
}