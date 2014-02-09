using System.Linq;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Nimbus.PoisonMessages;
using Nimbus.UnitTests.BatchSendingTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BatchSendingTests
{
    [TestFixture]
    public class WhenPublishingACollectionOfEvents : SpecificationForAsync<Bus>
    {
        private IEventSender _eventSender;

        public override Task<Bus> Given()
        {
            var logger = Substitute.For<ILogger>();
            var commandSender = Substitute.For<ICommandSender>();
            var requestSender = Substitute.For<IRequestSender>();
            var multicastRequestSender = Substitute.For<IMulticastRequestSender>();
            _eventSender = Substitute.For<IEventSender>();
            var messagePumps = new MessagePump[0];
            var deadLetterQueues = Substitute.For<IDeadLetterQueues>();

            var bus = new Bus(logger, commandSender, requestSender, multicastRequestSender, _eventSender, messagePumps, deadLetterQueues);
            return Task.FromResult(bus);
        }

        public override async Task When()
        {
            var events = new IBusEvent[] {new FooEvent(), new BarEvent(), new BazEvent()};
            await Subject.PublishAll(events);
        }

        [Test]
        public void TheEventSenderShouldHaveReceivedThreeCalls()
        {
            _eventSender.ReceivedCalls().Count().ShouldBe(3);
        }
    }
}