using System.Threading.Tasks;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.RequestResponseTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.RequestResponseTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MessageBrokerTests.RequestResponseTests
{
    [TestFixture]
    public class WhenResolvingAHandlersForASimpleRequest : TestForAll<IRequestHandlerFactory>
    {
        private OwnedComponent<IHandleRequest<FooRequest, FooResponse>> _handler;

        protected override async Task When()
        {
            _handler = Subject.GetHandler<FooRequest, FooResponse>();
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ThereShouldBeAFooRequestHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            _handler.Component.ShouldBeTypeOf<FooRequestHandler>();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            _handler.Dispose();
        }
    }
}