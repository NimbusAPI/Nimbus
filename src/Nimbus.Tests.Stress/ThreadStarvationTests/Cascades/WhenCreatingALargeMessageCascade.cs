using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Infrastructure.Logging;
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
        public new const int TimeoutSeconds = 300;
        public const int NumberOfDoThingACommands = 10;

        private const int _expectedMessageCount = NumberOfDoThingACommands*ThingAHappenedEventHandler.NumberOfDoThingBCommands*ThingBHappenedEventHandler.NumberOfDoThingCCommands;

        private MethodCallCounter MethodCallCounter { get; set; }

        protected override async Task<Bus> Given()
        {
            //var logger = TestHarnessLoggerFactory.Create();
            var logger = new NullLogger();

            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithTransport(new InProcessTransportConfiguration())
                                      .WithTypesFrom(typeProvider)
                                      .WithGlobalInboundInterceptorTypes(typeProvider.InterceptorTypes.Where(t => typeof(IInboundInterceptor).IsAssignableFrom(t)).ToArray())
                                      .WithGlobalOutboundInterceptorTypes(typeProvider.InterceptorTypes.Where(t => typeof(IOutboundInterceptor).IsAssignableFrom(t)).ToArray())
                                      .WithDependencyResolver(new DependencyResolver(typeProvider))
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(TimeoutSeconds))
                                      .WithLogger(logger)
                                      .WithDebugOptions(
                                          dc =>
                                              dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                  "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            MethodCallCounter = MethodCallCounter.CreateInstance(bus.InstanceId);

            await bus.Start(MessagePumpTypes.All);

            return bus;
        }

        protected override async Task When()
        {
            Console.WriteLine("Expecting {0} {1}s", _expectedMessageCount, typeof(DoThingCCommand).Name);

            var commands = Enumerable.Range(0, NumberOfDoThingACommands)
                                     .Select(i => new DoThingACommand())
                                     .ToArray();

            await Subject.SendAll(commands);

            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.OfType<DoThingCCommand>().Count() >= _expectedMessageCount);
        }

        [Test]
        public async Task TheCorrectNumberOfMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages.OfType<DoThingCCommand>().Count().ShouldBe(_expectedMessageCount);
        }

        public override void TestFixtureTearDown()
        {
            var bus = Subject;
            if (bus != null) MethodCallCounter.DestroyInstance(bus.InstanceId);

            base.TestFixtureTearDown();
        }
    }
}