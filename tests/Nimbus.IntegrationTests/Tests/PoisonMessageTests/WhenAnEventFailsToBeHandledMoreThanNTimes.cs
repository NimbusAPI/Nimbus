using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.EventHandlers;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests
{
    [TestFixture]
    public class WhenAnEventFailsToBeHandledMoreThanNTimes : TestForBus
    {
        private GoBangEvent _goBangEvent;
        private string _someContent;
        private NimbusMessage[] _deadLetterMessages;

        private int _maxDeliveryAttempts;

        protected override async Task Given(IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await base.Given(scenario);
            _maxDeliveryAttempts = Instance.Configuration.MaxDeliveryAttempts;
        }

        protected override async Task When()
        {
            _someContent = Guid.NewGuid().ToString();
            _goBangEvent = new GoBangEvent(_someContent);

            var expectedMessageCount = (_maxDeliveryAttempts * 1) + (1 * 1);    // one set of _maxDeliveryAttempts because of failure and one set of one because of success

            await Bus.Publish(_goBangEvent);
            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedCalls.Count() >= expectedMessageCount);

            _deadLetterMessages = await Bus.DeadLetterOffice.PopAll(1, TimeSpan.FromSeconds(TimeoutSeconds));
        }

        [Test]
        [TestCaseSource(typeof(AllBusConfigurations<WhenAnEventFailsToBeHandledMoreThanNTimes>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task ThereShouldBeExactlyOneMessageOnTheDeadLetterQueue()
        {
            _deadLetterMessages.Count().ShouldBe(1);
        }

        [Then]
        public async Task ThePayloadOfTheMessageShouldBeTheOriginalEventThatWentBang()
        {
            ((GoBangEvent) _deadLetterMessages.Single().Payload).SomeContent.ShouldBe(_someContent);
        }

        [Then]
        public async Task TheMessageShouldHaveTheCorrectNumberOfDeliveryAttempts()
        {
            var nimbusMessage = _deadLetterMessages.Single();
            nimbusMessage.DeliveryAttempts.Count().ShouldBe(_maxDeliveryAttempts);
        }

        [Then]
        public async Task TheNonExplodingEventHandlerShouldHaveSeenTheMessageOnce()
        {
            var nonExplodingCalls = MethodCallCounter.ReceivedCallsWithAnyArg<DoesNotGoBangCompetingEventHandler>(h => h.Handle(_goBangEvent)).ToArray();
            nonExplodingCalls.Length.ShouldBe(1);
        }
    }
}