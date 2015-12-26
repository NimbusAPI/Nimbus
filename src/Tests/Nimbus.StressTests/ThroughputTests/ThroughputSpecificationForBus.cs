using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.Logging;
using Nimbus.StressTests.ThroughputTests.EventHandlers;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    [Timeout(60*1000)]
    public abstract class ThroughputSpecificationForBus
    {
        private Stopwatch _stopwatch;
        private double _messagesPerSecond;
        private double _averageOneWayLatency;
        private double _averageRequestResponseLatency;

        protected Bus Bus { get; private set; }

        protected virtual int NumMessagesToSend => 1*1000;

        protected virtual async Task Given(BusBuilderConfiguration busBuilderConfiguration)
        {
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
            StressTestMessageHandler.Reset(NumMessagesToSend);
            _stopwatch = Stopwatch.StartNew();

            await Task.WhenAll(SendMessages(Bus));

            Console.WriteLine();
            Console.WriteLine("Finished sending messages. Waiting for them to all find their way back...");
            StressTestMessageHandler.WaitUntilDone(TimeSpan.FromSeconds(60));
            _stopwatch.Stop();

            Console.WriteLine("All done. Took {0} milliseconds to process {1} messages", _stopwatch.ElapsedMilliseconds, NumMessagesToSend);

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
        public async Task Run(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();
        }

        public abstract Task SendMessages(IBus bus);

        [TearDown]
        public void TearDown()
        {
            var bus = Bus;
            bus?.Dispose();
        }
    }
}