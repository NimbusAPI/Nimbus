using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Logger;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.StartupPerformanceTests
{
    [TestFixture]
    public class WhenCreatingABigBus
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        [Timeout(10*60*1000)]
        public async Task TheStartupTimeShouldBeAcceptable()
        {
            var logger = new ConsoleLogger();
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var messageHandlerFactory = new DefaultMessageHandlerFactory(typeProvider);

            using (new AssertingStopwatch("First bus creation", TimeSpan.MaxValue))
            {
                using (var bus = new BusBuilder().Configure()
                                                 .WithNames("MyTestSuite", Environment.MachineName)
                                                 .WithConnectionString(CommonResources.ConnectionString)
                                                 .WithTypesFrom(typeProvider)
                                                 .WithDefaultHandlerFactory(messageHandlerFactory)
                                                 .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                                 .WithLogger(logger)
                                                 .WithDebugOptions(
                                                     dc =>
                                                         dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                             "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                                 .Build())
                {
                    bus.Start();
                }
            }

            using (new AssertingStopwatch("Subsequent bus creation", TimeSpan.MaxValue))
            {
                using (var bus = new BusBuilder().Configure()
                                                 .WithNames("MyTestSuite", Environment.MachineName)
                                                 .WithConnectionString(CommonResources.ConnectionString)
                                                 .WithTypesFrom(typeProvider)
                                                 .WithDefaultHandlerFactory(messageHandlerFactory)
                                                 .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                                 .WithLogger(logger)
                                                 .Build())
                {
                    bus.Start();
                }
            }
        }
    }
}