using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.Interceptors;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests
{
    [TestFixture]
    [Timeout(15*1000)]
    public class WhenSendingACascadeOfCommands : TestForBus
    {
        private const int _numExpectedMessages = 3;

        private IDispatchContext[] _dispatchContexts;
        private NimbusMessage[] _nimbusMessages;

        protected override Task Given(BusBuilderConfiguration busBuilderConfiguration)
        {
            TestInterceptor.Clear();

            return base.Given(busBuilderConfiguration);
        }

        protected override async Task When()
        {
            var someCommand = new FirstCommand();
            await Bus.Send(someCommand);
            await TimeSpan.FromSeconds(10).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= _numExpectedMessages);

            _dispatchContexts = TestInterceptor.DispatchContexts.ToArray();
            _nimbusMessages = TestInterceptor.NimbusMessages.ToArray();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task TheCorrectNumberOfMessagesShouldHaveBeenObserved(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _nimbusMessages.Count().ShouldBe(_numExpectedMessages);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task WeShouldObserveOneDispatchContextPerMessage(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _dispatchContexts.Count().ShouldBe(_numExpectedMessages);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task AllParentMessageIdsShouldBeDifferent(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _dispatchContexts.GroupBy(x => x.ResultOfMessageId).Count().ShouldBe(_numExpectedMessages);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task TheCorrelationIdsShouldAllBeTheSame(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _dispatchContexts.GroupBy(x => x.CorrelationId).Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task TheThirdMessageShouldBeCausedByTheSecondMessage(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _nimbusMessages[1].Properties[MessagePropertyKeys.PrecedingMessageId].ShouldBe(_nimbusMessages[0].MessageId);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task TheSecondMessageShouldBeCausedByTheFirstMessage(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _nimbusMessages[2].Properties[MessagePropertyKeys.PrecedingMessageId].ShouldBe(_nimbusMessages[1].MessageId);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingACascadeOfCommands>))]
        public async Task TheFirstMessageShouldBeTheInitialMessage(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _nimbusMessages[0].Properties[MessagePropertyKeys.PrecedingMessageId].ShouldBe(null);
        }
    }
}