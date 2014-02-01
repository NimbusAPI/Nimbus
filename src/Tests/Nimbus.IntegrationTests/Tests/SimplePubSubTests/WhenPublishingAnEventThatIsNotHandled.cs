using System.Threading.Tasks;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    public class WhenPublishingAnEventThatIsNotHandled : TestForAllBuses
    {
        public override async Task When()
        {
            var myEvent = new SomeEventWeDoNotHandle();
            await Bus.Publish(myEvent);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async Task NoExceptionIsThrown(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            Should.NotThrow(async () => await When());
        }
    }
}