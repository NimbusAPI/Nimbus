using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.IntegrationTests.Tests.ThreadStarvationTests.MessageContracts;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.Logger;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.ThreadStarvationTests
{
    [Timeout(_timeoutSeconds*1000)]
    public class WhenSmashingTheBusForMoreThanTheMessageLockDuration : SpecificationForAsync<Bus>
    {
        private const int _timeoutSeconds = 120;
        private const int _secondsToRun = 30;
        private static readonly TimeSpan _messageLockDuration = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _timeToRun = TimeSpan.FromSeconds(_secondsToRun);
        private RollingLogger _logger;
        private bool _abort;
        private int _numMessagesSent;

        protected override async Task<Bus> Given()
        {
            _logger = new RollingLogger();
            _logger.Storing += OnLoggerStoring;

            var logger = _logger;
            //var logger = new ConsoleLogger();

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
            _numMessagesSent = 0;

            try
            {
                var sw = Stopwatch.StartNew();
                var tasks = new List<Task>();
                for (var i = 0; i < Environment.ProcessorCount*2; i++)
                {
                    var task = Task.Run(async () =>
                                              {
                                                  while (sw.Elapsed < _timeToRun)
                                                  {
                                                      var commands = Enumerable.Range(0, 100)
                                                                               .Select(j => new CommandThatWillFloodTheBus())
                                                                               .ToArray();

                                                      await Subject.SendAll(commands);
                                                      Interlocked.Add(ref _numMessagesSent, 100);

                                                      await Task.Delay(TimeSpan.FromSeconds(1));

                                                      if (_abort) return;
                                                  }
                                              });
                    tasks.Add(task);

                    await Task.WhenAll(tasks);

                    await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => _abort || MethodCallCounter.AllReceivedCalls.Count() >= _numMessagesSent);
                }
            }
            finally
            {
                _logger.Dump();
            }

            Console.WriteLine("Messages sent: {0}", _numMessagesSent);
        }

        private void OnLoggerStoring(object sender, string e)
        {
            if (e.Contains("MessageLockLostException")) _abort = true;
        }

        [Test]
        public async Task WeShouldNeverSeeAMessageLockLostException()
        {
            _abort.ShouldBe(false);
        }

        [Test]
        public async Task WeShouldHaveReceivedTheCorrectNumberOfCommands()
        {
            MethodCallCounter.AllReceivedMessages.OfType<CommandThatWillFloodTheBus>().Count().ShouldBe(_numMessagesSent);
        }
    }
}