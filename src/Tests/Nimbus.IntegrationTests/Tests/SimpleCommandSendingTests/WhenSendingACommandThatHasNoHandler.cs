using System.Threading.Tasks;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests
{
    [TestFixture]
    public class WhenSendingACommandThatHasNoHandler : TestForAllBuses
    {
        public override async Task When()
        {
            var someCommand = new SomeCommandThatHasNoHandler();
            await Bus.Send(someCommand);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async Task NothingShouldGoBang(ITestHarnessBusFactory busFactory)
        {
            await Given(busFactory);
            await When();
        }
    }
}