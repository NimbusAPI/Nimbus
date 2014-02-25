using System.Threading.Tasks;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MessageBrokerTests.CommandBrokerTests
{
    [TestFixture]
    public class WhenResolvingAHandlerForASimpleCommand : TestForAll<ICommandHandlerFactory>
    {
        private OwnedComponent<IHandleCommand<FooCommand>> _handler;

        protected override async Task When()
        {
            _handler = Subject.GetHandler<FooCommand>();
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task TheHandlerTypeShouldBeCorrect(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            _handler.Component.ShouldBeTypeOf<BrokerTestCommandHandler>();
        }

        [TearDown]
        public override void TearDown()
        {
            _handler.Dispose();
            base.TearDown();
        }
    }
}