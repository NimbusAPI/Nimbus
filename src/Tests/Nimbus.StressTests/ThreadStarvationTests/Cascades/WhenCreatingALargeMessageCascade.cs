using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.StressTests.ThreadStarvationTests.Cascades.Handlers;
using Nimbus.StressTests.ThreadStarvationTests.Cascades.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Transports.InProcess;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.StressTests.ThreadStarvationTests.Cascades
{
    [Timeout(TimeoutSeconds*1000)]
    public class WhenCreatingALargeMessageCascade : SpecificationForAsync<Bus>
    {
        public const int TimeoutSeconds = 300;
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

            await Enumerable.Range(0, NumberOfDoThingACommands)
                            .Select(i => Subject.Send(new DoThingACommand()))
                            .WhenAll();
        }

        [Test]
        public async Task TheCorrectNumberOfMessagesShouldHaveBeenObserved()
        {
            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.OfType<DoThingCCommand>().Count() >= _expectedMessageCount);
            MethodCallCounter.AllReceivedMessages.OfType<DoThingCCommand>().Count().ShouldBe(_expectedMessageCount);
        }
    }
}