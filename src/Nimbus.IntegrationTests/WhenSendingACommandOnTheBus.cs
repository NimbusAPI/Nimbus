using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Nimbus.IntegrationTests.EventTests;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.MessageContracts;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    public class WhenSendingACommandOnTheBus : SpecificationForBus
    {
        public override void When()
        {
            var someCommand = new SomeCommand();
            Subject.Send(someCommand).Wait();
            TimeSpan.FromSeconds(1).SleepUntil(() => _commandBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void TheCommandBrokerShouldReceiveThatCommand()
        {
            _commandBroker.Received().Dispatch(Arg.Any<SomeCommand>());
        }
    }
}