using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.Interceptors;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests
{
    [TestFixture]
    public class WhenSendingACascadeOfCommands : TestForBus
    {
        private const int _numExpectedMessages = 3;

        private IDispatchContext[] _dispatchContexts;
        private NimbusMessage[] _nimbusMessages;

        protected override Task Given(IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            TestInterceptor.Clear();

            return base.Given(scenario);
        }

        protected override async Task When()
        {
            var someCommand = new FirstCommand();
            await Bus.Send(someCommand);
            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= _numExpectedMessages);

            _dispatchContexts = TestInterceptor.DispatchContexts.ToArray();
            _nimbusMessages = TestInterceptor.NimbusMessages.ToArray();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task TheCorrectNumberOfMessagesShouldHaveBeenObserved(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _nimbusMessages.Count().ShouldBe(_numExpectedMessages);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task WeShouldObserveOneDispatchContextPerMessage(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _dispatchContexts.Count().ShouldBe(_numExpectedMessages);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task AllParentMessageIdsShouldBeDifferent(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _dispatchContexts.GroupBy(x => x.ResultOfMessageId).Count().ShouldBe(_numExpectedMessages);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task TheCorrelationIdsShouldAllBeTheSame(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _dispatchContexts.GroupBy(x => x.CorrelationId).Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task TheThirdMessageShouldBeCausedByTheSecondMessage(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _nimbusMessages[1].PrecedingMessageId.ShouldBe(_nimbusMessages[0].MessageId);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task TheSecondMessageShouldBeCausedByTheFirstMessage(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _nimbusMessages[2].PrecedingMessageId.ShouldBe(_nimbusMessages[1].MessageId);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task TheFirstMessageShouldBeTheInitialMessage(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _nimbusMessages[0].PrecedingMessageId.ShouldBe(null);
        }
    }
}