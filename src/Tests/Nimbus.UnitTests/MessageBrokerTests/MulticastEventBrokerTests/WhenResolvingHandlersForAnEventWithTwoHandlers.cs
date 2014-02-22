using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.MulticastEventBrokerTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.MulticastEventBrokerTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MessageBrokerTests.MulticastEventBrokerTests
{
    [TestFixture]
    public class WhenResolvingHandlersForAnEventWithTwoHandlers : TestForAll<IMulticastEventHandlerFactory>
    {
        private OwnedComponent<IEnumerable<IHandleMulticastEvent<FooEvent>>> _handlers;

        protected override async Task When()
        {
            _handlers = Subject.GetHandlers<FooEvent>();
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ThereShouldBeAFirstFooEventHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            _handlers.Component.OfType<FirstFooEventHandler>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ThereShouldBeASecondFooEventHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            _handlers.Component.OfType<SecondFooEventHandler>().Count().ShouldBe(1);
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