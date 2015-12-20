using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Handlers;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.NimbusMessageServices;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.DispatcherTests
{
    [TestFixture]
    internal class WhenDispatchingTwoCommands : SpecificationForAsync<CommandMessageDispatcher>
    {
        private NimbusMessageFactory _nimbusMessageFactory;

        private readonly Guid _id1 = new Guid();
        private readonly Guid _id2 = new Guid();

        private const int _expectedCallCount = 2;

        protected override async Task<CommandMessageDispatcher> Given()
        {
            var clock = new SystemClock();
            var logger = new ConsoleLogger();
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var serializer = new DataContractSerializer(typeProvider);
            var replyQueueNameSetting = new ReplyQueueNameSetting(
                new ApplicationNameSetting {Value = "TestApplication"},
                new InstanceNameSetting {Value = "TestInstance"});

            var handlerMap = new HandlerMapper(typeProvider).GetFullHandlerMap(typeof (IHandleCommand<>));

            _nimbusMessageFactory = new NimbusMessageFactory(new DefaultMessageTimeToLiveSetting(),
                                                             replyQueueNameSetting,
                                                             clock,
                                                             new DispatchContextManager());

            return new CommandMessageDispatcher(new DependencyResolver(typeProvider),
                                                new NullInboundInterceptorFactory(),
                                                new NullLogger(),
                                                handlerMap,
                                                Substitute.For<IPropertyInjector>());
        }

        protected override async Task When()
        {
            MethodCallCounter.Clear();

            var command1 = new FooCommand(_id1);
            var command2 = new FooCommand(_id2);

            await Subject.Dispatch(await _nimbusMessageFactory.Create("nullQueue", command1));
            await Subject.Dispatch(await _nimbusMessageFactory.Create("nullQueue", command2));

            MethodCallCounter.Stop();
        }

        [Test]
        public async Task Command1ShouldBeDispatchedToTheCorrectHandler()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(null))
                             .Select(c => c.Single())
                             .Cast<FooCommand>()
                             .Select(c => c.Id)
                             .ShouldContain(_id1);
        }

        [Test]
        public async Task Command2ShouldBeDispatchedToTheCorrectHandler()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(null))
                             .Select(c => c.Single())
                             .Cast<FooCommand>()
                             .Select(c => c.Id)
                             .ShouldContain(_id2);
        }

        [Test]
        public async Task ATotalOfTwoCallsToHandleShouldBeReceived()
        {
            var calls = MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(null));
            calls.Count().ShouldBe(_expectedCallCount);
        }

        [Test]
        public async Task BothInstancesOfTheCommandHandlerShouldHaveBeenDisposed()
        {
            var calls = MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Dispose());
            calls.Count().ShouldBe(_expectedCallCount);
        }
    }
}