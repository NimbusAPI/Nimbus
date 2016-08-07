using System;
using System.Linq;
using System.Threading.Tasks;
using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.IntegrationTests.Tests.InterceptorTests.Handlers;
using Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors;
using Nimbus.IntegrationTests.Tests.InterceptorTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Transports.InProcess;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests
{
    [TestFixture]
    public class WhenSendingACommandThatHasAMethodAndClassLevelInterceptor : SpecificationForAsync<Bus>
    {
        private readonly string _globalPrefix = Guid.NewGuid().ToString();
        private MethodCallCounter MethodCallCounter { get; set; }
        private const int _expectedTotalCallCount = 11; // 5 interceptors * 2 + 1 handler

        protected override async Task<Bus> Given()
        {
            var testFixtureType = GetType();
            var typeProvider = new TestHarnessTypeProvider(new[] {testFixtureType.Assembly}, new[] {testFixtureType.Namespace});
            var logger = TestHarnessLoggerFactory.Create();

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithGlobalPrefix(_globalPrefix)
                                      .WithTransport(new InProcessTransportConfiguration())
                                      .WithTypesFrom(typeProvider)
                                      .WithDependencyResolver(new DependencyResolver(typeProvider))
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(TimeoutSeconds))
                                      .WithMaxDeliveryAttempts(1)
                                      .WithGlobalInboundInterceptorTypes(typeof (SomeGlobalInterceptor))
                                      .WithLogger(logger)
                                      .WithDebugOptions(
                                          dc =>
                                          dc.RemoveAllExistingNamespaceElementsOnStartup(
                                              "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();

            MethodCallCounter = MethodCallCounter.CreateInstance(bus.InstanceId);
            MethodCallCounter.Clear();

            await bus.Start();

            return bus;
        }

        protected override async Task When()
        {
            await Subject.Send(new FooCommand());
            await Timeout.WaitUntil(() => MethodCallCounter.TotalReceivedCalls >= _expectedTotalCallCount);

            MethodCallCounter.Stop();
            MethodCallCounter.Dump();
        }

        public override void TearDown()
        {
            var bus = Subject;
            if (bus != null) MethodCallCounter.DestroyInstance(bus.InstanceId);
            base.TearDown();
        }

        [Test]
        public async Task TheCommandBrokerShouldReceiveThatCommand()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<MultipleCommandHandler>(h => h.Handle((FooCommand) null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheMethodLevelExecutingInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeMethodLevelInterceptorForFoo>(i => i.OnCommandHandlerExecuting<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheMethodLevelSuccessInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeMethodLevelInterceptorForFoo>(i => i.OnCommandHandlerSuccess<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheBaseMethodLevelExecutingInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeBaseMethodLevelInterceptorForFoo>(i => i.OnCommandHandlerExecuting<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheBaseMethodLevelSuccessInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeBaseMethodLevelInterceptorForFoo>(i => i.OnCommandHandlerSuccess<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheBaseClassLevelExecutingInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeBaseClassLevelInterceptor>(i => i.OnCommandHandlerExecuting<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheBaseClassLevelSuccessInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeBaseClassLevelInterceptor>(i => i.OnCommandHandlerSuccess<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheClassLevelExecutingInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeClassLevelInterceptor>(i => i.OnCommandHandlerExecuting<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheClassLevelSuccessInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeClassLevelInterceptor>(i => i.OnCommandHandlerSuccess<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheGlobalExecutingInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeGlobalInterceptor>(i => i.OnCommandHandlerExecuting<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheGlobalSuccessInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeGlobalInterceptor>(i => i.OnCommandHandlerSuccess<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheCorrectNumberOfInterceptorsShouldHaveBeenInvoked()
        {
            MethodCallCounter.TotalReceivedCalls.ShouldBe(_expectedTotalCallCount);
        }
    }
}