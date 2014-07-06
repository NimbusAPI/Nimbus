using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests
{
    [TestFixture]
    [Timeout(15*1000)]
    public class WhenSendingACommandThatHasAnAbstractBaseType : TestForBus
    {
        protected override async Task When()
        {
            var someCommand = new SomeConcreteCommandType();
            await Bus.Send(someCommand);
            await TimeSpan.FromSeconds(5).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        public async Task TheCommandBrokerShouldReceiveThatCommand()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeConcreteCommandType>().Count().ShouldBe(1);
        }

        [Test]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }
    }
}