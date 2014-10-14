using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.Logger.Serilog;
using Nimbus.StressTests.ThreadStarvationTests.MessageContracts;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Serilog;
using Shouldly;

namespace Nimbus.StressTests.ThreadStarvationTests
{
    [Timeout(_timeoutSeconds*1000)]
    public class WhenCreatingALargeMessageCascade : SpecificationForAsync<Bus>
    {
        private const int _timeoutSeconds = 180;
        private static readonly TimeSpan _messageLockDuration = TimeSpan.FromSeconds(30);
        private const int _numInitialMessages = 10;

        private const int _expectedMessageCount = 1000;

        protected override async Task<Bus> Given()
        {
            var log = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .WriteTo.Seq("http://localhost:5341")
                .MinimumLevel.Debug()
                .CreateLogger();

            var logger = new SerilogLogger(log);

            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ServiceBusConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithGlobalInboundInterceptorTypes(typeProvider.InterceptorTypes.Where(t => typeof (IInboundInterceptor).IsAssignableFrom(t)).ToArray())
                                      .WithGlobalOutboundInterceptorTypes(typeProvider.InterceptorTypes.Where(t => typeof (IOutboundInterceptor).IsAssignableFrom(t)).ToArray())
                                      .WithDependencyResolver(new DependencyResolver(typeProvider))
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithDefaultMessageLockDuration(_messageLockDuration)
                                      .WithLogger(logger)
                                      .WithDebugOptions(
                                          dc =>
                                          dc.RemoveAllExistingNamespaceElementsOnStartup(
                                              "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            await bus.Start(MessagePumpTypes.All);

            return bus;
        }

        protected override async Task When()
        {
            var tasks = Enumerable.Range(0, _numInitialMessages)
                                  .AsParallel()
                                  .Select(i => Subject.Send(new DoThingACommand()))
                                  .ToArray();

            await Task.WhenAll(tasks);

            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.OfType<DoThingCCommand>().Count() >= _expectedMessageCount);
        }

        [Test]
        public async Task TheCorrectNumberOfMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages.OfType<DoThingCCommand>().Count().ShouldBe(1000);
        }
    }
}