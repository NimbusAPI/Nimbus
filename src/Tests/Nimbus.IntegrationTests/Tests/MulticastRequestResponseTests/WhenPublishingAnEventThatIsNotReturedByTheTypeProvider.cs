using System;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.MessageContracts.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests
{
    public class WhenSendingAMulticastRequestThatIsNotReturedByTheTypeProvider : TestForAllBuses
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        public override async Task When(ITestHarnessBusFactory busFactory)
        {
            var bus = busFactory.Create();

            await bus.Request(new SomeRequestThatIsNotReturedByTheTypeProvider(), _timeout);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void ABusExceptionIsThrown(ITestHarnessBusFactory busFactory)
        {
            try
            {
                await When(busFactory);
                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                ex.ShouldBeTypeOf<BusException>();
                ex.Message.ShouldMatch(
                    @"^The type Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts.SomeRequestThatIsNotReturedByTheTypeProvider is not a recognised request type\. Ensure it has been registered with the builder with the WithTypesFrom method\.$");
            }
        }
    }
}