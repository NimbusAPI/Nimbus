using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
    public class WhenSmashingTheBusForMoreThanTheMessageLockDuration : SpecificationForAsync<Bus>
    {
        private const int _timeoutSeconds = 180;
        private const int _secondsToRun = 20;
        private static readonly TimeSpan _messageLockDuration = TimeSpan.FromSeconds(9);
        private readonly TimeSpan _timeToRun = TimeSpan.FromSeconds(_secondsToRun);
        private ILogger _logger;
        private bool _abort;
        private int _numMessagesSent;

        protected override async Task<Bus> Given()
        {
            var log = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341")
                .MinimumLevel.Debug()
                .CreateLogger();

            _logger = new SerilogLogger(log);

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
                                      .WithLogger(_logger)
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
            _numMessagesSent = 0;
            const int batchSize = 200;

            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < _timeToRun)
            {
                var commands = Enumerable.Range(0, batchSize)
                                         .Select(j => new CommandThatWillBlockTheThread())
                                         .ToArray();

                await Subject.SendAll(commands);
                Interlocked.Add(ref _numMessagesSent, batchSize);

                await Task.Delay(TimeSpan.FromSeconds(1));

                if (_abort) return;
            }

            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => _abort || MethodCallCounter.AllReceivedCalls.Count() >= _numMessagesSent);
        }

        [Test]
        public async Task WeShouldHaveReceivedTheCorrectNumberOfCommands()
        {
            Console.WriteLine("Messages sent: {0}", _numMessagesSent);

            var numMessagesReceived = MethodCallCounter.AllReceivedMessages.OfType<CommandThatWillBlockTheThread>().Count();
            Console.WriteLine("Messages received: {0}", numMessagesReceived);

            numMessagesReceived.ShouldBe(_numMessagesSent);
        }
    }
}