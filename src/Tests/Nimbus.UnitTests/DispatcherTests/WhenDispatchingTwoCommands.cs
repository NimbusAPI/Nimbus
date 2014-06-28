using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.Settings;
using Nimbus.Handlers;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Logger;
using Nimbus.Tests.Common;
using Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.DispatcherTests
{
    [TestFixture]
    public class WhenDispatchingTwoCommands : TestForAllDependencyResolvers
    {
        private CommandMessageDispatcher _commandDispatcher;
        private BrokeredMessageFactory _brokeredMessageFactory;

        private readonly Guid _id1 = new Guid();
        private readonly Guid _id2 = new Guid();

        protected override async Task Given(AllDependencyResolversTestContext context)
        {
            MethodCallCounter.Clear();

            await base.Given(context);

            var clock = new SystemClock();
            var serializer = new DataContractSerializer();
            var replyQueueNameSetting = new ReplyQueueNameSetting(
                new ApplicationNameSetting {Value = "TestApplication"},
                new InstanceNameSetting {Value = "TestInstance"});

            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var handlerMap = new HandlerMapper(typeProvider).GetFullHandlerMap(typeof(IHandleCommand<>));

            _brokeredMessageFactory = new BrokeredMessageFactory(new MaxLargeMessageSizeSetting(),
                                                                 new MaxSmallMessageSizeSetting(),
                                                                 replyQueueNameSetting,
                                                                 clock,
                                                                 new NullCompressor(),
                                                                 new NullDependencyResolver(),
                                                                 new DispatchContextManager(), 
                                                                 new UnsupportedLargeMessageBodyStore(),
                                                                 new NullOutboundInterceptorFactory(),
                                                                 serializer,
                                                                 typeProvider);

            _commandDispatcher = new CommandMessageDispatcher(_brokeredMessageFactory,
                                                              new SystemClock(),
                                                              Subject,
                                                              new NullInboundInterceptorFactory(),
                                                              new NullLogger(),
                                                              handlerMap);
        }

        protected override async Task When()
        {
            var command1 = new FooCommand(_id1);
            var command2 = new FooCommand(_id2);

            await _commandDispatcher.Dispatch(await _brokeredMessageFactory.Create(command1));
            await _commandDispatcher.Dispatch(await _brokeredMessageFactory.Create(command2));
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task Command1ShouldBeDispatchedToTheCorrectHandler(AllDependencyResolversTestContext context)
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
        public async Task Command2ShouldBeDispatchedToTheCorrectHandler(AllDependencyResolversTestContext context)
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
        public async Task ATotalOfTwoCallsToHandleShouldBeReceived(AllDependencyResolversTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(null)).Count().ShouldBe(2);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task BothInstancesOfTheCommandHandlerShouldHaveBeenDisposed(AllDependencyResolversTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Dispose()).Count().ShouldBe(2);
        }
    }
}