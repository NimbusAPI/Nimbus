using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.StressTests.ThroughputTests.EventHandlers;
using Nimbus.StressTests.ThroughputTests.Infrastructure;
using Nimbus.Tests.Common;
using Nimbus.Transports.InProcess;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    [Timeout(60*1000)]
    public abstract class ThroughputSpecificationForBus : SpecificationForAsync<Bus>
    {
        private TimeSpan _timeout;
        private AssemblyScanningTypeProvider _typeProvider;

        private FakeDependencyResolver _dependencyResolver;
        private FakeHandler _fakeHandler;
        private Stopwatch _stopwatch;
        private double _messagesPerSecond;
        private ILogger _logger;
        private string _largeMessageBodyTempPath;

        protected virtual int NumMessagesToSend
        {
            get { return 8000; }
        }

        protected abstract int ExpectedMessagesPerSecond { get; }

        protected override async Task<Bus> Given()
        {
            _largeMessageBodyTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Guid.NewGuid().ToString());

            _fakeHandler = new FakeHandler(NumMessagesToSend);
            _dependencyResolver = new FakeDependencyResolver(_fakeHandler);
            _timeout = TimeSpan.FromSeconds(300); //FIXME set to 30 seconds
            _typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            _logger = TestHarnessLoggerFactory.Create();
            //_logger = new NullLogger();

            var bus = new BusBuilder().Configure()
                                      .WithTransport(new InProcessTransportConfiguration())
                                      .WithNames("ThroughputTestSuite", Environment.MachineName)
                                      .WithLogger(_logger)
                                      .WithTypesFrom(_typeProvider)
                                      .WithDependencyResolver(_dependencyResolver)
                                      .WithDebugOptions(dc => dc.RemoveAllExistingNamespaceElementsOnStartup(
                                          "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            await bus.Start();
            return bus;
        }

        protected override async Task When()
        {
            Console.WriteLine("Starting to send messages...");
            _stopwatch = Stopwatch.StartNew();

            await Task.WhenAll(SendMessages(Subject));

            Console.WriteLine();
            Console.WriteLine("Finished sending messages. Waiting for them to all find their way back...");
            _fakeHandler.WaitUntilDone(_timeout);
            _stopwatch.Stop();

            Console.WriteLine("All done. Took {0} milliseconds to process {1} messages", _stopwatch.ElapsedMilliseconds, NumMessagesToSend);
            _messagesPerSecond = _fakeHandler.ActualNumMessagesReceived/_stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine("Average throughput: {0} messages/second", _messagesPerSecond);
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            Subject.Stop().Wait();
            _dependencyResolver = null;
            base.TestFixtureTearDown();
            if (Directory.Exists(_largeMessageBodyTempPath)) Directory.Delete(_largeMessageBodyTempPath, true);
        }

        public abstract IEnumerable<Task> SendMessages(IBus bus);

        [Test]
        public async Task TheCorrectNumberOfMessagesShouldHaveBeenObserved()
        {
            _fakeHandler.ActualNumMessagesReceived.ShouldBe(_fakeHandler.ExpectedNumMessagesReceived);
        }

        [Test]
        public async Task WeShouldGetAcceptableThroughput()
        {
            _messagesPerSecond.ShouldBeGreaterThan(ExpectedMessagesPerSecond);
        }
    }
}