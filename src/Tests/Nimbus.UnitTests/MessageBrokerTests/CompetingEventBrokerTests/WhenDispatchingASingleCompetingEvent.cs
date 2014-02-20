using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.CompetingEventBrokerTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.CompetingEventBrokerTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MessageBrokerTests.CompetingEventBrokerTests
{
    [TestFixture]
    public class WhenDispatchingASingleCompetingEvent : TestForAll<ICompetingEventBroker>
    {
        private FooEvent _fooEvent;

        protected override async Task When()
        {
            _fooEvent = new FooEvent();

            Subject.PublishCompeting(_fooEvent);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task TheFirstEventHandlerShouldReceiveACopy(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<FirstFooEventHandler>(h => h.Handle(_fooEvent)).ShouldContain(_fooEvent);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task TheSecondEventHandlerShouldReceiveACopy(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<SecondFooEventHandler>(h => h.Handle(_fooEvent)).ShouldContain(_fooEvent);
        }
    }
}