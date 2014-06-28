using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.InterceptorTests.Handlers;
using Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors;
using Nimbus.IntegrationTests.Tests.InterceptorTests.MessageContracts;
using Nimbus.Logger;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests
{
    [TestFixture]
    [Timeout(15*1000)]
    public class WhenSendingACommandThatHasAMethodAndClassLevelInterceptor : SpecificationForAsync<IBus>
    {
        private const int _expectedTotalMessageCount = 11; // 5 interceptors * 2 + 1 handler

        protected override async Task<IBus> Given()
        {
            MethodCallCounter.Clear();

            var testFixtureType = GetType();
            var typeProvider = new TestHarnessTypeProvider(new[] {testFixtureType.Assembly}, new[] {testFixtureType.Namespace});
            var logger = new ConsoleLogger();

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ServiceBusConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithDependencyResolver(new DependencyResolver(typeProvider))
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithMaxDeliveryAttempts(1)
                                      .WithGlobalInboundInterceptorTypes(typeof (SomeGlobalInterceptor))
                                      .WithLogger(logger)
                                      .WithDebugOptions(
                                          dc =>
                                          dc.RemoveAllExistingNamespaceElementsOnStartup(
                                              "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            await bus.Start();

            return bus;
        }

        protected override async Task When()
        {
            await Subject.Send(new FooCommand());
            TimeSpan.FromSeconds(10).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= _expectedTotalMessageCount);
        }

        [Test]
        public async Task TheCommandBrokerShouldReceiveThatCommand()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<MultipleCommandHandler>(h => h.Handle((FooCommand) null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheMethodLevelInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeMethodLevelInterceptorForFoo>(i => i.OnCommandHandlerExecuting<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheBaseMethodLevelInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeBaseMethodLevelInterceptorForFoo>(i => i.OnCommandHandlerExecuting<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheBaseClassLevelInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeBaseClassLevelInterceptor>(i => i.OnCommandHandlerExecuting<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheClassLevelInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeClassLevelInterceptor>(i => i.OnCommandHandlerExecuting<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheGlobalInterceptorShouldHaveBeenInvoked()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeGlobalInterceptor>(i => i.OnCommandHandlerExecuting<FooCommand>(null, null)).Count().ShouldBe(1);
        }

        [Test]
        public async Task NoOtherMethodLevelInterceptorsShouldHaveBeenInvoked()
        {
            MethodCallCounter.AllReceivedMessages.OfType<FooCommand>().Count().ShouldBe(_expectedTotalMessageCount);
        }
    }
}