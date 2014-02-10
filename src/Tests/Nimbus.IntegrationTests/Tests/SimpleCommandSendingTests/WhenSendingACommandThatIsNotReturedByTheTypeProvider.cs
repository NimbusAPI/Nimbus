using System;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.MessageContracts.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests
{
    public class WhenSendingACommandThatIsNotReturedByTheTypeProvider : TestForAllBuses
    {
        public override async Task When()
        {
            var myCommand = new SomeCommandThatIsNotReturedByTheTypeProvider();
            await Bus.Send(myCommand);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async Task ABusExceptionIsThrown(ITestHarnessBusFactory busFactory)
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
                    @"^The type Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts.SomeCommandThatIsNotReturedByTheTypeProvider is not a recognised command type\. Ensure it has been registered with the builder with the WithTypesFrom method\.$");
            }
        }
    }
}