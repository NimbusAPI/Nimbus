using System;
using System.Linq;
using System.Threading.Tasks;
using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.StressTests.ThreadStarvationTests.CommandHandlersSendingOtherCommands.Handlers;
using Nimbus.StressTests.ThreadStarvationTests.CommandHandlersSendingOtherCommands.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Transports.WindowsServiceBus;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.StressTests.ThreadStarvationTests.CommandHandlersSendingOtherCommands
{
    internal class WhenACommandHandlerSmashesTheBusForMoreThanItsLockDuration : SpecificationForAsync<Bus>
    {
        private const int _timeoutSeconds = 180;
        private static readonly TimeSpan _messageLockDuration = TimeSpan.FromSeconds(20);

        protected override async Task<Bus> Given()
        {
            var logger = TestHarnessLoggerFactory.Create();

            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            var bus = new BusBuilder().Configure()
                                      .WithTransport(new WindowsServiceBusTransportConfiguration()
                                                         .WithConnectionString(DefaultSettingsReader.Get<AzureServiceBusConnectionString>())
                )
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithTypesFrom(typeProvider)
                                      .WithGlobalInboundInterceptorTypes(typeProvider.InterceptorTypes.Where(t => typeof (IInboundInterceptor).IsAssignableFrom(t)).ToArray())
                                      .WithGlobalOutboundInterceptorTypes(typeProvider.InterceptorTypes.Where(t => typeof (IOutboundInterceptor).IsAssignableFrom(t)).ToArray())
                                      .WithDependencyResolver(new DependencyResolver(typeProvider))
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
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
            SmashTheBusCommandHandler.NumCommandsSent = 0;
            await Subject.Send(new SmashTheBusCommand());
            await Task.Delay(SmashTheBusCommandHandler.HammerTheBusFor);

            var remainingTime = TimeSpan.FromSeconds(_timeoutSeconds) - SmashTheBusCommandHandler.HammerTheBusFor;
            await remainingTime.WaitUntil(() => MethodCallCounter.AllReceivedMessages.OfType<NoOpCommand>().Count() >= SmashTheBusCommandHandler.NumCommandsSent);
        }

        [Test]
        public async Task NoMessagesShouldHaveLocksExpire()
        {
            MethodCallCounter.AllReceivedMessages.OfType<NoOpCommand>().Count().ShouldBe(SmashTheBusCommandHandler.NumCommandsSent);
        }
    }
}