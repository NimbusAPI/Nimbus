using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.StressTests.ThreadStarvationTests.BadlyBehavedHandlersThatDoNotKnowAboutAsync.MessageContracts;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Transports.InProcess;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.StressTests.ThreadStarvationTests.BadlyBehavedHandlersThatDoNotKnowAboutAsync
{
    public class WhenSendingABunchOfCommandsThatWillSaturateTheThreadPool : SpecificationForAsync<Bus>
    {
        private ILogger _logger;
        private int _numMessagesToSend;

        private MethodCallCounter MethodCallCounter { get; set; }

        protected override async Task<Bus> Given()
        {
            _logger = TestHarnessLoggerFactory.Create();

            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            var bus = new BusBuilder().Configure()
                                      .WithTransport(new InProcessTransportConfiguration())
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithTypesFrom(typeProvider)
                                      .WithGlobalInboundInterceptorTypes(typeProvider.InterceptorTypes.Where(t => typeof(IInboundInterceptor).IsAssignableFrom(t)).ToArray())
                                      .WithGlobalOutboundInterceptorTypes(typeProvider.InterceptorTypes.Where(t => typeof(IOutboundInterceptor).IsAssignableFrom(t)).ToArray())
                                      .WithDependencyResolver(new DependencyResolver(typeProvider))
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(TimeoutSeconds))
                                      .WithLogger(_logger)
                                      .WithDebugOptions(
                                          dc =>
                                              dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                  "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();

            MethodCallCounter = MethodCallCounter.CreateInstance(bus.InstanceId);
            await bus.Start();

            return bus;
        }

        protected override async Task When()
        {
            _numMessagesToSend = new ConcurrentHandlerLimitSetting().Default;

            var commands = Enumerable.Range(0, _numMessagesToSend)
                                     .Select(j => new CommandThatWillBlockTheThread())
                                     .ToArray();

            await Subject.SendAll(commands);

            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedCalls.Count() >= _numMessagesToSend);
        }

        [Test]
        public async Task WeShouldHaveReceivedTheCorrectNumberOfCommands()
        {
            var numMessagesReceived = MethodCallCounter.AllReceivedMessages.OfType<CommandThatWillBlockTheThread>().Count();
            Console.WriteLine("Messages received: {0}", numMessagesReceived);

            numMessagesReceived.ShouldBe(_numMessagesToSend);
        }

        public override void TestFixtureTearDown()
        {
            var bus = Subject;
            if (bus != null) MethodCallCounter.DestroyInstance(bus.InstanceId);

            base.TestFixtureTearDown();
        }
    }
}