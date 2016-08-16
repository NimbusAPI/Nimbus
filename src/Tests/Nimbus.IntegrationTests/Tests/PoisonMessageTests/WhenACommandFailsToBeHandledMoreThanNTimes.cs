using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests
{
    [TestFixture]
    public class WhenACommandFailsToBeHandledMoreThanNTimes : TestForBus
    {
        private GoBangCommand _goBangCommand;
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
            _goBangCommand = new GoBangCommand(_someContent);

            await Bus.Send(_goBangCommand);
            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedCalls.Count() >= _maxDeliveryAttempts);

            _deadLetterMessages = await Bus.DeadLetterOffice.PopAll(1, TimeSpan.FromSeconds(TimeoutSeconds));
        }

        [Test]
        [TestCaseSource(typeof(AllBusConfigurations<WhenACommandFailsToBeHandledMoreThanNTimes>))]
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
        public async Task ThePayloadOfTheMessageShouldBeTheOriginalCommandThatWentBang()
        {
            ((GoBangCommand) _deadLetterMessages.Single().Payload).SomeContent.ShouldBe(_someContent);
        }

        [Then]
        public async Task TheMessageShouldHaveTheCorrectNumberOfDeliveryAttempts()
        {
            // if the transport layer is handling retry attempts then we don't get to record delivery attempts
            if (Instance.Configuration.RequireRetriesToBeHandledBy == RetriesHandledBy.Transport) return;

            var nimbusMessage = _deadLetterMessages.Single();
            nimbusMessage.DeliveryAttempts.Count().ShouldBe(_maxDeliveryAttempts);
        }
    }
}