using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.CommandHandlers;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.IntegrationTests.TestScenarioGeneration;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests
{
    [TestFixture]
    [Timeout(_timeoutSeconds*1000)]
    public class WhenSendingACommandWhoseHandlerRequiresSomeProperties : TestForBus
    {
        private const int _timeoutSeconds = 5;

        protected override async Task When()
        {
            SomeOtherCommandHandler.Clear();
            var someCommand = new SomeOtherCommand();
            await Bus.Send(someCommand);
            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        [TestCaseSource(typeof (TestForAllBusConfigurations<WhenSendingACommandWhoseHandlerRequiresSomeProperties>))]
        public async Task TheCommandBrokerShouldReceiveThatCommand(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            MethodCallCounter.AllReceivedMessages.OfType<SomeOtherCommand>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (TestForAllBusConfigurations<WhenSendingACommandWhoseHandlerRequiresSomeProperties>))]
        public async Task TheDispatchContextShouldBeSet(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            SomeOtherCommandHandler.ReceivedDispatchContext.ShouldNotBe(null);
        }

        [Test]
        [TestCaseSource(typeof (TestForAllBusConfigurations<WhenSendingACommandWhoseHandlerRequiresSomeProperties>))]
        public async Task TheMessagePropertiesShouldBeSet(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            SomeOtherCommandHandler.ReceivedMessageProperties.ShouldNotBe(null);
        }

        [Test]
        [TestCaseSource(typeof (TestForAllBusConfigurations<WhenSendingACommandWhoseHandlerRequiresSomeProperties>))]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}