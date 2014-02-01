using System;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using Nimbus.MessageContracts.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    public class WhenPublishingAnEventThatIsNotReturedByTheTypeProvider : TestForAllBuses
    {
        public override async Task When()
        {
            var myEvent = new SomeEventThatIsNotReturedByTheTypeProvider();
            await Bus.Publish(myEvent);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void ABusExceptionIsThrown(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);

            try
            {
                await When();
                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                ex.ShouldBeTypeOf<BusException>();
                ex.Message.ShouldMatch(
                    @"^The type Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts.SomeEventThatIsNotReturedByTheTypeProvider is not a recognised event type\. Ensure it has been registered with the builder with the WithTypesFrom method\.$");
            }
        }
    }
}