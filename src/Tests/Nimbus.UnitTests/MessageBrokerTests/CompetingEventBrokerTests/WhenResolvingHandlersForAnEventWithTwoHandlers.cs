using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.CompetingEventBrokerTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.CompetingEventBrokerTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MessageBrokerTests.CompetingEventBrokerTests
{
    [TestFixture]
    public class WhenResolvingHandlersForAnEventWithTwoHandlers : TestForAll<ICompetingEventHandlerFactory>
    {
        private OwnedComponent<IEnumerable<IHandleCompetingEvent<FooEvent>>> _handlers;

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