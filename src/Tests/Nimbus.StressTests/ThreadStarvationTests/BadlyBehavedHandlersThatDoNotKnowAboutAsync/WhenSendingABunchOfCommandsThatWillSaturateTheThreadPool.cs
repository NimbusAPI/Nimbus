using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.StressTests.ThreadStarvationTests.BadlyBehavedHandlersThatDoNotKnowAboutAsync.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Transports.InProcess;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.StressTests.ThreadStarvationTests.BadlyBehavedHandlersThatDoNotKnowAboutAsync
{
    [Timeout(_timeoutSeconds*1000)]
    public class WhenSendingABunchOfCommandsThatWillSaturateTheThreadPool : SpecificationForAsync<Bus>
    {
        private const int _timeoutSeconds = 180;

        private ILogger _logger;
        private int _numMessagesToSend;

        protected override async Task<Bus> Given()
        {
            _logger = TestHarnessLoggerFactory.Create();

            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            var bus = new BusBuilder().Configure()
                                      .WithTransport(new InProcessTransportConfiguration())
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithTypesFrom(typeProvider)
                                      .WithGlobalInboundInterceptorTypes(typeProvider.InterceptorTypes.Where(t => typeof (IInboundInterceptor).IsAssignableFrom(t)).ToArray())
                                      .WithGlobalOutboundInterceptorTypes(typeProvider.InterceptorTypes.Where(t => typeof (IOutboundInterceptor).IsAssignableFrom(t)).ToArray())
                                      .WithDependencyResolver(new DependencyResolver(typeProvider))
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithLogger(_logger)
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
            _numMessagesToSend = (Environment.ProcessorCount*2)*Environment.ProcessorCount;

            var commands = Enumerable.Range(0, _numMessagesToSend)
                                     .Select(j => new CommandThatWillBlockTheThread())
                                     .ToArray();

            await Subject.SendAll(commands);

            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedCalls.Count() >= _numMessagesToSend);
        }

        [Test]
        public async Task WeShouldHaveReceivedTheCorrectNumberOfCommands()
        {
            var numMessagesReceived = MethodCallCounter.AllReceivedMessages.OfType<CommandThatWillBlockTheThread>().Count();
            Console.WriteLine("Messages received: {0}", numMessagesReceived);

            numMessagesReceived.ShouldBe(_numMessagesToSend);
        }
    }
}