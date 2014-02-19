using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;

namespace Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests
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