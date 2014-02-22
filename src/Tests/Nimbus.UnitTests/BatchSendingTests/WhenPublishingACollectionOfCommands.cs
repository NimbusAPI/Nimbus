using System.Linq;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.MessageContracts;
using Nimbus.PoisonMessages;
using Nimbus.UnitTests.BatchSendingTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BatchSendingTests
{
    [TestFixture]
    public class WhenPublishingACollectionOfCommands : SpecificationForAsync<Bus>
    {
        private ICommandSender _commandSender;

        public override Task<Bus> Given()
        {
            var logger = Substitute.For<ILogger>();
            _commandSender = Substitute.For<ICommandSender>();
            var requestSender = Substitute.For<IRequestSender>();
            var multicastRequestSender = Substitute.For<IMulticastRequestSender>();
            var eventSender = Substitute.For<IEventSender>();
            var messagePumps = new MessagePump[0];
            var deadLetterQueues = Substitute.For<IDeadLetterQueues>();

            var bus = new Bus(logger, _commandSender, requestSender, multicastRequestSender, eventSender, messagePumps, deadLetterQueues);
            return Task.FromResult(bus);
        }

        public override async Task When()
        {
            var commands = new IBusCommand[] {new FooCommand(), new BarCommand(), new BazCommand()};
            await Subject.SendAll(commands);
        }

        [Test]
        public void TheCommandSenderShouldHaveReceivedThreeCalls()
        {
            _commandSender.ReceivedCalls().Count().ShouldBe(3);
        }
    }
}