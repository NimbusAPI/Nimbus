using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.HandlerFactories;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.DispatcherTests
{
    [TestFixture]
    public class WhenDispatchingTwoCommands : TestForAll<ICommandHandlerFactory>
    {
        private CommandMessageDispatcher _commandDispatcher;

        private readonly Guid _id1 = new Guid();
        private readonly Guid _id2 = new Guid();

        protected override async Task Given(AllSubjectsTestContext context)
        {
            await base.Given(context);

            _commandDispatcher = new CommandMessageDispatcher(Subject, typeof (FooCommand), new SystemClock());
        }

        protected override async Task When()
        {
            var command1 = new FooCommand(_id1);
            var command2 = new FooCommand(_id2);

            await _commandDispatcher.Dispatch(new BrokeredMessage(command1));
            await _commandDispatcher.Dispatch(new BrokeredMessage(command2));
        }

#pragma warning disable 4014

        [Test]
        [TestCaseSource("TestCases")]
        public async Task Command1ShouldBeDispatchedToTheCorrectHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(null))
                             .Select(args => args[0])
                             .Cast<FooCommand>()
                             .Select(c => c.Id)
                             .ShouldContain(_id1);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task Command2ShouldBeDispatchedToTheCorrectHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(null))
                             .Select(args => args[0])
                             .Cast<FooCommand>()
                             .Select(c => c.Id)
                             .ShouldContain(_id2);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ATotalOfTwoCallsToHandleShouldBeReceived(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(null)).Count().ShouldBe(2);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task BothInstancesOfTheCommandHandlerShouldHaveBeenDisposed(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Dispose()).Count().ShouldBe(2);
        }
    }
}