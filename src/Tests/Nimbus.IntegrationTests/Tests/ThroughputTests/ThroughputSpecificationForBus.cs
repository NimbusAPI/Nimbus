using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.IntegrationTests.Tests.ThroughputTests.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests
{
    [TestFixture]
    [Explicit("We pay $$ for messages when we're hitting the Azure Message Bus. Let's not run these on CI builds.")]
    [Timeout(60 * 1000)]
    public abstract class ThroughputSpecificationForBus : SpecificationFor<Bus>
    {
        private TimeSpan _timeout;
        private AssemblyScanningTypeProvider _typeProvider;

        private FakeBroker _broker;
        private Stopwatch _stopwatch;
        private double _messagesPerSecond;

        protected const int NumMessagesToSend = 4*1000;
        protected abstract int ExpectedMessagesPerSecond { get; }

        public override async Task<Bus> Given()
        {
            _broker = new FakeBroker(NumMessagesToSend);
            _timeout = TimeSpan.FromSeconds(30);
            _typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            var bus = new BusBuilder().Configure()
                                      .WithNames("ThroughputTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(_typeProvider)
                                      .WithCommandHandlerFactory(_broker)
                                      .WithRequestBroker(_broker)
                                      .WithMulticastRequestBroker(_broker)
                                      .WithMulticastEventBroker(_broker)
                                      .WithCompetingEventBroker(_broker)
                                      .WithDebugOptions(
                                          dc =>
                                              dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                  "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            bus.Start();
            return bus;
        }

        public override async Task When()
        {
            Console.WriteLine("Starting to send messages...");
            _stopwatch = Stopwatch.StartNew();

            await Task.WhenAll(SendMessages(Subject));

            Console.WriteLine();
            Console.WriteLine("Finished sending messages. Waiting for them to all find their way back...");
            _broker.WaitUntilDone(_timeout);
            _stopwatch.Stop();

            Console.WriteLine("All done. Took {0} milliseconds to process {1} messages", _stopwatch.ElapsedMilliseconds, NumMessagesToSend);
            _messagesPerSecond = _broker.ActualNumMessagesReceived/_stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine("Average throughput: {0} messages/second", _messagesPerSecond);
        }

        public abstract IEnumerable<Task> SendMessages(IBus bus);

        [Test]
        public async Task TheCorrectNumberOfMessagesShouldHaveBeenObserved()
        {
            Subject = await Given();
            await When();

            _broker.ActualNumMessagesReceived.ShouldBe(_broker.ExpectedNumMessagesReceived);
        }

        [Test]
        public async Task WeShouldGetAcceptableThroughput()
        {
            Subject = await Given();
            await When();

            _messagesPerSecond.ShouldBeGreaterThan(ExpectedMessagesPerSecond);
        }

        [TearDown]
        public override void TearDown()
        {
            Subject.Stop();
            _broker = null;
        }
    }
}