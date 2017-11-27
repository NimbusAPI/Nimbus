using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Handlers;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.Serialization;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
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
        private Guid BusId { get; set; }
        private MethodCallCounter MethodCallCounter { get; set; }

        private const int _expectedCallCount = 2;

        protected override async Task<CommandMessageDispatcher> Given()
        {
            BusId = Guid.NewGuid();
            MethodCallCounter = MethodCallCounter.CreateInstance(BusId);

            var clock = new SystemClock();
            var logger = new ConsoleLogger();
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var serializer = new DataContractSerializer(typeProvider);
            var replyQueueNameSetting = new ReplyQueueNameSetting(
                new ApplicationNameSetting {Value = "TestApplication"},
                new InstanceNameSetting {Value = "TestInstance"},
                new PathFactory(new GlobalPrefixSetting()));
            var propertyInjector = new StubPropertyInjector(BusId);

            var handlerMap = new HandlerMapper(typeProvider).GetFullHandlerMap(typeof(IHandleCommand<>));

            _nimbusMessageFactory = new NimbusMessageFactory(new DefaultMessageTimeToLiveSetting(),
                                                             replyQueueNameSetting,
                                                             clock,
                                                             new DispatchContextManager());

            return new CommandMessageDispatcher(new DependencyResolver(typeProvider),
                                                new NullInboundInterceptorFactory(),
                                                new NullLogger(),
                                                handlerMap,
                                                propertyInjector);
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

        public override void TearDown()
        {
            MethodCallCounter.DestroyInstance(BusId);
            base.TearDown();
        }

        private class StubPropertyInjector : IPropertyInjector
        {
            private readonly Guid _busId;

            public StubPropertyInjector(Guid busId)
            {
                _busId = busId;
            }

            public void Inject(object handlerOrInterceptor, NimbusMessage nimbusMessage)
            {
                var requiresBusId = handlerOrInterceptor as IRequireBusId;
                if (requiresBusId != null) requiresBusId.BusId = _busId;
            }
        }
    }
}