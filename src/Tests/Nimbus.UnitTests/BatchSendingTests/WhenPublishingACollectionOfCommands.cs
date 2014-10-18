using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Infrastructure.TaskScheduling;
using Nimbus.MessageContracts;
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

        protected override Task<Bus> Given()
        {
            var logger = Substitute.For<ILogger>();
            _commandSender = Substitute.For<ICommandSender>();
            var requestSender = Substitute.For<IRequestSender>();
            var multicastRequestSender = Substitute.For<IMulticastRequestSender>();
            var eventSender = Substitute.For<IEventSender>();
            var messagePumpsManager = Substitute.For<IMessagePumpsManager>();
            var deadLetterQueues = Substitute.For<IDeadLetterQueues>();
            var taskFactory = new NimbusTaskFactory(new MaximumThreadPoolThreadsSetting(), new MinimumThreadPoolThreadsSetting(), logger);

            var bus = new Bus(logger, _commandSender, requestSender, multicastRequestSender, eventSender, messagePumpsManager, deadLetterQueues, taskFactory);
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