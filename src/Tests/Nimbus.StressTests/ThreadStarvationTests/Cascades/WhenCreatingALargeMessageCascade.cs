using System;
using System.Linq;
using System.Threading.Tasks;
using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.StressTests.Configuration;
using Nimbus.StressTests.ThreadStarvationTests.Cascades.Handlers;
using Nimbus.StressTests.ThreadStarvationTests.Cascades.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Transports.InProcess;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.StressTests.ThreadStarvationTests.Cascades
{
    [Timeout(_timeoutSeconds*1000)]
    public class WhenCreatingALargeMessageCascade : SpecificationForAsync<Bus>
    {
        private const int _timeoutSeconds = 300;
        private static readonly TimeSpan _messageLockDuration = TimeSpan.FromSeconds(30);
        public const int NumberOfDoThingACommands = 10;

        private const int _expectedMessageCount = NumberOfDoThingACommands*ThingAHappenedEventHandler.NumberOfDoThingBCommands*ThingBHappenedEventHandler.NumberOfDoThingCCommands;

        protected override async Task<Bus> Given()
        {
            var logger = TestHarnessLoggerFactory.Create();
            //var logger = new NullLogger();

            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithTransport(new InProcessTransportConfiguration())
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
            await bus.Start(MessagePumpTypes.All);

            return bus;
        }

        protected override async Task When()
        {
            Console.WriteLine("Expecting {0} {1}s", _expectedMessageCount, typeof (DoThingCCommand).Name);

            var tasks = Enumerable.Range(0, NumberOfDoThingACommands)
                                  .AsParallel()
                                  .Select(i => Subject.Send(new DoThingACommand()))
                                  .ToArray();

            await Task.WhenAll(tasks);

            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.OfType<DoThingCCommand>().Count() >= _expectedMessageCount);
        }

        [Test]
        public async Task TheCorrectNumberOfMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages.OfType<DoThingCCommand>().Count().ShouldBe(_expectedMessageCount);
        }
    }
}