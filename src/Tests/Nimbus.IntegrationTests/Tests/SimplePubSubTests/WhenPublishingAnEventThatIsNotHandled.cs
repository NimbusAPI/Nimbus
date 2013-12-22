using System.Threading.Tasks;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    public class WhenPublishingAnEventThatIsNotHandled : TestForAllBuses
    {
        public override async Task When(ITestHarnessBusFactory busFactory)
        {
            var bus = busFactory.Create();

            var myEvent = new SomeEventWeDoNotHandle();
            await bus.Publish(myEvent);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public void NoExceptionIsThrown(ITestHarnessBusFactory busFactory)
        {
            Should.NotThrow(async () => await When(busFactory));
        }
    }
}