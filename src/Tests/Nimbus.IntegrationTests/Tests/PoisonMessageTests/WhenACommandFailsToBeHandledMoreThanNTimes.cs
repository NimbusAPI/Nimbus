using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration;
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

        protected override Task Given(BusBuilderConfiguration busBuilderConfiguration)
        {
            _maxDeliveryAttempts = busBuilderConfiguration.MaxDeliveryAttempts;

            return base.Given(busBuilderConfiguration);
        }

        protected override async Task When()
        {
            _someContent = Guid.NewGuid().ToString();
            _goBangCommand = new GoBangCommand(_someContent);

            await Bus.Send(_goBangCommand);
            await TimeSpan.FromSeconds(10).WaitUntil(() => MethodCallCounter.AllReceivedCalls.Count() >= _maxDeliveryAttempts);

            _deadLetterMessages = await Bus.DeadLetterOffice.PopAll();
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenACommandFailsToBeHandledMoreThanNTimes>))]
        public async Task ThereShouldBeExactlyOneMessageOnTheDeadLetterQueue(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _deadLetterMessages.Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenACommandFailsToBeHandledMoreThanNTimes>))]
        public async Task ThePayloadOfTheMessageShouldBeTheOriginalCommandThatWentBang(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            ((GoBangCommand) _deadLetterMessages.Single().Payload).SomeContent.ShouldBe(_someContent);
        }

        [Test]
        [TestCaseSource(typeof(AllBusConfigurations<WhenACommandFailsToBeHandledMoreThanNTimes>))]
        public async Task TheMessageShouldHaveTheCorrectNumberOfDeliveryAttempts(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            var nimbusMessage = _deadLetterMessages.Single();
            nimbusMessage.DeliveryAttempts.Count().ShouldBe(_maxDeliveryAttempts);
        }
    }
}