using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Logging;
using Nimbus.StressTests.ThroughputTests.EventHandlers;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;
using Serilog;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    [Timeout(TimeoutSeconds*1000)]
    public abstract class ThroughputSpecificationForBus
    {
        protected const int TimeoutSeconds = 300;

        private Stopwatch _sendingStopwatch;
        private Stopwatch _sendingAndReceivingStopwatch;

        private int _numMessagesSent;
        private double _messagesPerSecond;
        private double _averageOneWayLatency;
        private double? _averageRequestResponseLatency;
        private ScenarioInstance<BusBuilderConfiguration> _instance;

        protected Bus Bus { get; private set; }

        protected virtual TimeSpan SendMessagesFor { get; } = TimeSpan.FromSeconds(5);

        static ThroughputSpecificationForBus()
        {
            TestHarnessLoggerFactory.Create();
        }

        protected virtual async Task Given(IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            _instance = scenario.CreateInstance();
            _numMessagesSent = 0;

            var busBuilderConfiguration = _instance.Configuration;

            if (!Debugger.IsAttached)
            {
                busBuilderConfiguration.WithLogger(new NullLogger());
            }

            Bus = busBuilderConfiguration.Build();
            Log.Debug("Starting bus...");
            await Bus.Start();
            Log.Debug("Bus started.");
        }

        protected virtual async Task When()
        {
            Log.Information("Starting to send messages...");
            StressTestMessageHandler.Reset();
            _sendingStopwatch = Stopwatch.StartNew();
            _sendingAndReceivingStopwatch = Stopwatch.StartNew();

            await Enumerable.Range(0, 20)
                            .Select(i => Task.Run(() => SendMessages(Bus)).ConfigureAwaitFalse())
                            .WhenAll();

            _sendingStopwatch.Stop();

            Log.Information("Total of {NumMessagesSent} messages sent in {Elapsed}", _numMessagesSent, _sendingStopwatch.Elapsed);

            await StressTestMessageHandler.WaitUntilDone(_numMessagesSent, TimeSpan.FromSeconds(TimeoutSeconds));
            _sendingAndReceivingStopwatch.Stop();
        }

        public abstract Task SendMessages(IBus bus);

        protected void IncrementExpectedMessageCount(int numMessages = 1)
        {
            Interlocked.Add(ref _numMessagesSent, numMessages);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<ThroughputSpecificationForBus>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _messagesPerSecond = StressTestMessageHandler.ActualNumMessagesReceived/_sendingAndReceivingStopwatch.Elapsed.TotalSeconds;
            _averageOneWayLatency = StressTestMessageHandler.Messages
                                                            .Select(m => m.WhenReceived - m.WhenSent)
                                                            .Select(ts => ts.TotalMilliseconds)
                                                            .Average();
            _averageRequestResponseLatency = StressTestMessageHandler.ResponseMessages.Any()
                ? StressTestMessageHandler.ResponseMessages
                                          .Select(m => m.WhenReceived - m.RequestSentAt)
                                          .Select(ts => ts.TotalMilliseconds)
                                          .Average()
                : default(double?);

            Log.Information("Total of {NumMessagesSent} messages processed in {Elapsed}", _numMessagesSent, _sendingAndReceivingStopwatch.Elapsed);
            Log.Information("Average throughput: {MessagesPerSecond} messages/second", _messagesPerSecond);
            Log.Information("Average one-way latency: {AverageOneWayLatency}", TimeSpan.FromMilliseconds(_averageOneWayLatency));
            Log.Information("Average request/response latency: {AverageRequestResponseLatency}",
                            _averageRequestResponseLatency.HasValue
                                ? (object) TimeSpan.FromMilliseconds(_averageRequestResponseLatency.Value)
                                : "N/A");

            RecordTeamCityStatistic(testName, "TotalMessages", _numMessagesSent);
            RecordTeamCityStatistic(testName, "TotalElapsedMilliseconds", _sendingAndReceivingStopwatch.ElapsedMilliseconds);
            RecordTeamCityStatistic(testName, "MessagesPerSecond", _messagesPerSecond);
            RecordTeamCityStatistic(testName, "AverageOneWayLatencyInMilliseconds", _averageOneWayLatency);
            RecordTeamCityStatistic(testName, "AverageRequestResponseLatencyInMilliseconds", _averageRequestResponseLatency);
        }

        private void RecordTeamCityStatistic(string testName, string key, double? value)
        {
            var fullKey = string.Join(".", GetType().Name, testName, key);
            var message = "##teamcity[buildStatisticValue key='{0}' value='{1}']".FormatWith(fullKey, value);
            Console.WriteLine(message);
        }

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