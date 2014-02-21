using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests
{
    [TestFixture]
    public class WhenResolvingHandlersForAMulticastRequestWithTwoHandlers : TestForAll<IMulticastRequestHandlerFactory>
    {
        private OwnedComponent<IEnumerable<IHandleRequest<FooRequest, FooResponse>>> _handlers;

        protected override async Task When()
        {
            _handlers = Subject.GetHandlers<FooRequest, FooResponse>();
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ThereShouldBeAFirstFooRequestHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            _handlers.Component.OfType<FirstFooRequestHandler>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ThereShouldBeASecondFooRequestHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            _handlers.Component.OfType<SecondFooRequestHandler>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ThereShouldBeTwoHandlers(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            _handlers.Component.Count().ShouldBe(2);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            _handlers.Dispose();
        }
    }
}