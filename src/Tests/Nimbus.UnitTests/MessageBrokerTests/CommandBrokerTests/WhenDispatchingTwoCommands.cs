using System.Linq;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

#pragma warning disable 4014

namespace Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests
{
    [TestFixture]
    public class WhenDispatchingTwoCommands : TestForAll<ICommandHandlerFactory>
    {
        private FooCommand _command1;
        private FooCommand _command2;

        protected override async Task When()
        {
            _command1 = new FooCommand();
            _command2 = new FooCommand();

            using (var handler = Subject.GetHandler<FooCommand>())
            {
                await Task.WhenAll(handler.Component.Handle(_command1),
                                   handler.Component.Handle(_command2)
                    );
            }
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task Command1ShouldBeDispatchedToTheCorrectHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(_command1)).Contains(_command1).ShouldBe(true);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task Command2ShouldBeDispatchedToTheCorrectHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(_command2)).Contains(_command2).ShouldBe(true);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ATotalOfTwoCallsShouldBeReceived(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(null)).Count().ShouldBe(2);
        }
    }
}