using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.CommandHandlers;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
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
        public async Task TheCommandBrokerShouldReceiveThatCommand()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeOtherCommand>().Count().ShouldBe(1);
        }

        [Test]
        public async Task TheDispatchContextShouldBeSet()
        {
            SomeOtherCommandHandler.ReceivedDispatchContext.ShouldNotBe(null);
        }

        [Test]
        public async Task TheMessagePropertiesShouldBeSet()
        {
            SomeOtherCommandHandler.ReceivedMessageProperties.ShouldNotBe(null);
        }

        [Test]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}