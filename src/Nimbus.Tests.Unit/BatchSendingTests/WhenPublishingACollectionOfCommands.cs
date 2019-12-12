using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.Heartbeat;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Nimbus.Tests.Unit.BatchSendingTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.BatchSendingTests
{
    [TestFixture]
    public class WhenPublishingACollectionOfCommands : SpecificationForAsync<Bus>
    {
        private ICommandSender _commandSender;

        protected override Task<Bus> Given()
        {
            var logger = Substitute.For<ILogger>();
            _commandSender = Substitute.For<ICommandSender>();
            var requestSender = Substitute.For<IRequestSender>();
            var multicastRequestSender = Substitute.For<IMulticastRequestSender>();
            var eventSender = Substitute.For<IEventSender>();
            var messagePumpsManager = Substitute.For<IMessagePumpsManager>();
            var deadLetterOffice = Substitute.For<IDeadLetterOffice>();

            var bus = new Bus(logger,
                              _commandSender,
                              requestSender,
                              multicastRequestSender,
                              eventSender,
                              messagePumpsManager,
                              deadLetterOffice,
                              Substitute.For<IHeartbeat>());
            return Task.FromResult(bus);
        }

        protected override async Task When()
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