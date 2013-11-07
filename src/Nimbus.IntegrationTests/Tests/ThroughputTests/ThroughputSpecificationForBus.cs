using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.Tests.ThroughputTests.Infrastructure;
using Nimbus.IntegrationTests.Tests.ThroughputTests.MessageContracts;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests
{
    [TestFixture]
    [Ignore("We pay $$ for messages when we're hitting the Azure Message Bus. Let's not run these on CI builds.")]
    public abstract class ThroughputSpecificationForBus : SpecificationFor<Bus>
    {
        private TimeSpan _timeout;
        private AssemblyScanningTypeProvider _typeProvider;

        private FakeBroker _broker;
        private Stopwatch _stopwatch;
        private double _messagesPerSecond;

        protected const int NumMessagesToSend = 4*1000;
        protected abstract int ExpectedMessagesPerSecond { get; }

        public override Bus Given()
        {
            _broker = new FakeBroker(NumMessagesToSend);
            _timeout = TimeSpan.FromSeconds(30);
            _typeProvider = new AssemblyScanningTypeProvider(typeof (FooEvent).Assembly);

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(_typeProvider)
                                      .WithCommandBroker(_broker)
                                      .WithRequestBroker(_broker)
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

        public override void When()
        {
            Console.WriteLine("Starting to send messages...");
            _stopwatch = Stopwatch.StartNew();

            SendMessages(Subject).WaitAll();

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
        public void TheCorrectNumberOfMessagesShouldHaveBeenObserved()
        {
            _broker.ActualNumMessagesReceived.ShouldBe(_broker.ExpectedNumMessagesReceived);
        }

        [Test]
        public void WeShouldGetAcceptableThroughput()
        {
            _messagesPerSecond.ShouldBeGreaterThan(ExpectedMessagesPerSecond);
        }

        [TearDown]
        public void TearDown()
        {
            Subject.Stop();
            _broker = null;
        }
    }
}