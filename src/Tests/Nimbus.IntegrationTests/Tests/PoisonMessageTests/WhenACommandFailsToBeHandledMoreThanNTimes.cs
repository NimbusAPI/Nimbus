using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
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
            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedCalls.Count() >= _maxDeliveryAttempts);

            _deadLetterMessages = await Bus.DeadLetterOffice.PopAll(1, TimeSpan.FromSeconds(TimeoutSeconds));
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenACommandFailsToBeHandledMoreThanNTimes>))]
        public async Task ThereShouldBeExactlyOneMessageOnTheDeadLetterQueue(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _deadLetterMessages.Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenACommandFailsToBeHandledMoreThanNTimes>))]
        public async Task ThePayloadOfTheMessageShouldBeTheOriginalCommandThatWentBang(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            ((GoBangCommand) _deadLetterMessages.Single().Payload).SomeContent.ShouldBe(_someContent);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenACommandFailsToBeHandledMoreThanNTimes>))]
        public async Task TheMessageShouldHaveTheCorrectNumberOfDeliveryAttempts(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            var nimbusMessage = _deadLetterMessages.Single();
            nimbusMessage.DeliveryAttempts.Count().ShouldBe(_maxDeliveryAttempts);
        }
    }
}