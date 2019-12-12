using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Integration.Tests.SimpleDispatchContextCorrelationTests.Interceptors;
using Nimbus.Tests.Integration.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.SimpleDispatchContextCorrelationTests
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

        protected override void Reconfigure()
        {
            Instance.Configuration
                    .WithGlobalInboundInterceptorTypes(typeof (TestInterceptor));

            base.Reconfigure();
        }

        protected override async Task When()
        {
            var someCommand = new FirstCommand();
            await Bus.Send(someCommand);
            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= _numExpectedMessages);

            _dispatchContexts = TestInterceptor.DispatchContexts.ToArray();
            _nimbusMessages = TestInterceptor.NimbusMessages.ToArray();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task TheCorrectNumberOfMessagesShouldHaveBeenObserved()
        {
            _nimbusMessages.Count().ShouldBe(_numExpectedMessages);
        }

        [Then]
        public async Task WeShouldObserveOneDispatchContextPerMessage()
        {
            _dispatchContexts.Count().ShouldBe(_numExpectedMessages);
        }

        [Then]
        public async Task AllParentMessageIdsShouldBeDifferent()
        {
            _dispatchContexts.GroupBy(x => x.ResultOfMessageId).Count().ShouldBe(_numExpectedMessages);
        }

        [Then]
        public async Task TheCorrelationIdsShouldAllBeTheSame()
        {
            _dispatchContexts.GroupBy(x => x.CorrelationId).Count().ShouldBe(1);
        }

        [Then]
        public async Task TheThirdMessageShouldBeCausedByTheSecondMessage()
        {
            _nimbusMessages[1].PrecedingMessageId.ShouldBe(_nimbusMessages[0].MessageId);
        }

        [Then]
        public async Task TheSecondMessageShouldBeCausedByTheFirstMessage()
        {
            _nimbusMessages[2].PrecedingMessageId.ShouldBe(_nimbusMessages[1].MessageId);
        }

        [Then]
        public async Task TheFirstMessageShouldBeTheInitialMessage()
        {
            _nimbusMessages[0].PrecedingMessageId.ShouldBe(null);
        }
    }
}