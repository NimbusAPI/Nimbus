using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.Heartbeat;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.MessageContracts;
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

        protected override Task<Bus> Given()
        {
            var logger = Substitute.For<ILogger>();
            var commandSender = Substitute.For<ICommandSender>();
            var requestSender = Substitute.For<IRequestSender>();
            var multicastRequestSender = Substitute.For<IMulticastRequestSender>();
            _eventSender = Substitute.For<IEventSender>();
            var messagePumpsManager = Substitute.For<IMessagePumpsManager>();
            var deadLetterOffice = Substitute.For<IDeadLetterOffice>();

            var bus = new Bus(logger,
                              commandSender,
                              requestSender,
                              multicastRequestSender,
                              _eventSender,
                              messagePumpsManager,
                              deadLetterOffice,
                              Substitute.For<IHeartbeat>());
            return Task.FromResult(bus);
        }

        protected override async Task When()
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