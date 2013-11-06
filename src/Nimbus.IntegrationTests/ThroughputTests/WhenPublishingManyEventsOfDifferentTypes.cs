using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.ThroughputTests.ThroughputTestMessageContracts;
using Shouldly;

namespace Nimbus.IntegrationTests.ThroughputTests
{
    [TestFixture]
    [Ignore("We pay $$ for messages when we're hitting the Azure Message Bus. Let's not run these on CI builds.")]
    public class WhenPublishingManyEventsOfDifferentTypes : SpecificationFor<Bus>
    {
        private const int _messageCount = 10 * 1000;

        private FakeBroker _broker;
        private Stopwatch _stopwatch;
        private double _messagesPerSecond;

        public override Bus Given()
        {
            _broker = new FakeBroker(_messageCount);

            var typeProvider = new AssemblyScanningTypeProvider(typeof(FooEvent).Assembly);

            var bus = new BusBuilder().Configure()
                                      .WithInstanceName(Environment.MachineName + ".MyTestSuite")
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(_broker)
                                      .WithRequestBroker(_broker)
                                      .WithEventBroker(_broker)
                                      .Build();
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var bus = Subject;

            Console.WriteLine("Starting to send messages...");
            _stopwatch = Stopwatch.StartNew();
            Enumerable.Range(0, _messageCount/4)    // we're publishing 4 messages per iteration
                      .AsParallel()
                      .Do(i => bus.Publish(new FooEvent()))
                      .Do(i => bus.Publish(new BarEvent()))
                      .Do(i => bus.Publish(new BazEvent()))
                      .Do(i => bus.Publish(new QuxEvent()))
                      .Do(t => Console.Write("."))
                      .Done();

            Console.WriteLine();
            Console.WriteLine("Finished sending messages. Waiting for them to all find their way back...");
            _broker.WaitUntilDone();
            _stopwatch.Stop();

            Console.WriteLine("All done. Took {0} milliseconds to process {1} messages", _stopwatch.ElapsedMilliseconds, _messageCount);
            _messagesPerSecond = _messageCount / _stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine("Average throughput: {0} messages/second", _messagesPerSecond);
        }

        [Test]
        public void WeShouldGetAcceptableThroughput()
        {
            _messagesPerSecond.ShouldBeGreaterThan(200);
        }
    }
}