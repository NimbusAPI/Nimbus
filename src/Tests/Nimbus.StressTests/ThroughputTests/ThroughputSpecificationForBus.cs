using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.Logging;
using Nimbus.StressTests.ThroughputTests.EventHandlers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    [Timeout(TimeoutSeconds*1000)]
    public abstract class ThroughputSpecificationForBus
    {
        protected const int TimeoutSeconds = 60;

        private Stopwatch _stopwatch;
        private int _numMessagesSent;
        private double _messagesPerSecond;
        private double _averageOneWayLatency;
        private double _averageRequestResponseLatency;
        private ScenarioInstance<BusBuilderConfiguration> _instance;

        protected Bus Bus { get; private set; }

        protected virtual TimeSpan SendMessagesFor { get; } = TimeSpan.FromSeconds(5);

        protected void ExpectToReceiveMessages(int numMessages = 1)
        {
            Interlocked.Add(ref _numMessagesSent, numMessages);
        }

        protected virtual async Task Given(IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            _instance = scenario.CreateInstance();
            _numMessagesSent = 0;

            var busBuilderConfiguration = _instance.Configuration;

            // make sure we set this back to the defaults - we turn it down for most of the regression suite
            // so that the bus can stop more quickly.
            busBuilderConfiguration.WithDefaultConcurrentHandlerLimit(new ConcurrentHandlerLimitSetting().Value);

            if (!Debugger.IsAttached)
            {
                busBuilderConfiguration.WithLogger(new NullLogger());
            }

            Bus = busBuilderConfiguration.Build();
            await Bus.Start();
        }

        protected virtual async Task When()
        {
            Console.WriteLine("Starting to send messages...");
            StressTestMessageHandler.Reset();
            _stopwatch = Stopwatch.StartNew();

            await Task.WhenAll(SendMessages(Bus));

            Console.WriteLine();
            Console.WriteLine("Finished sending messages. Waiting for them to all find their way back...");
            StressTestMessageHandler.WaitUntilDone(_numMessagesSent, TimeSpan.FromSeconds(TimeoutSeconds));
            _stopwatch.Stop();

            Console.WriteLine("All done. Took {0} milliseconds to process {1} messages", _stopwatch.ElapsedMilliseconds, _numMessagesSent);

            _messagesPerSecond = StressTestMessageHandler.ActualNumMessagesReceived/_stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine("Average throughput: {0} messages/second", _messagesPerSecond);

            _averageOneWayLatency = StressTestMessageHandler.Messages
                                                            .Select(m => m.WhenReceived - m.WhenSent)
                                                            .Select(ts => ts.TotalMilliseconds)
                                                            .Average();
            Console.WriteLine("Average one-way latency: {0} milliseconds", _averageOneWayLatency);

            if (StressTestMessageHandler.ResponseMessages.Any())
            {
                _averageRequestResponseLatency = StressTestMessageHandler.ResponseMessages
                                                                         .Select(m => m.WhenReceived - m.RequestSentAt)
                                                                         .Select(ts => ts.TotalMilliseconds)
                                                                         .Average();
                Console.WriteLine("Average request/response latency: {0} milliseconds", _averageRequestResponseLatency);
            }
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<ThroughputSpecificationForBus>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
        }

        public abstract Task SendMessages(IBus bus);

        [TearDown]
        public void TearDown()
        {
            var bus = Bus;
            Bus = null;
            bus?.Dispose();

            _instance?.Dispose();
            _instance = null;
        }
    }
}