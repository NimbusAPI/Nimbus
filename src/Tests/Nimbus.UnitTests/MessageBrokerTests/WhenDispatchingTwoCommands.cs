using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.UnitTests.MessageBrokerTests
{
    [TestFixture]
    public class WhenDispatchingTwoCommands : TestForAll<ICommandBroker>
    {
        private BrokerTestCommand _command;

        protected override async Task When()
        {
            _command = new BrokerTestCommand();
            Subject.Dispatch(_command);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ItShouldBeDispatchedToTheCorrectHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(_command));
        }
    }
}