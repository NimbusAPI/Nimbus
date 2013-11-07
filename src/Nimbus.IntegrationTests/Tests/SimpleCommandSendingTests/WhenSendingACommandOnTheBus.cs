using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests
{
    [TestFixture]
    public class WhenSendingACommandOnTheBus : SpecificationForBus
    {
        public override void When()
        {
            var someCommand = new SomeCommand();
            Subject.Send(someCommand).Wait();
            TimeSpan.FromSeconds(10).SleepUntil(() => CommandBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void TheCommandBrokerShouldReceiveThatCommand()
        {
            CommandBroker.Received().Dispatch(Arg.Any<SomeCommand>());
        }
    }
}