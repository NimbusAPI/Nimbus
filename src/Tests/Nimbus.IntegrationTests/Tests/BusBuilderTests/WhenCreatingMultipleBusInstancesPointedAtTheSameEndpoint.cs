using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Logger;
using Nimbus.Logging;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.BusBuilderTests
{
    [TestFixture]
    [Timeout(120*1000)]
    public class WhenCreatingMultipleBusInstancesPointedAtTheSameEndpoint
    {
        private Bus[] _buses;

        [Test]
        public async Task NoneOfThemShouldGoBang()
        {
            var tasks = Enumerable.Range(0, 5)
                                  .Select(i => BuildMeABus())
                                  .ToArray();

            await Task.WhenAll(tasks);

            _buses = tasks.Select(t => t.Result).ToArray();
        }

        [SetUp]
        public void SetUp()
        {
            Task.Run(async () => { await ClearMeABus(); }).Wait();
        }

        [TearDown]
        public void TearDown()
        {
            _buses
                .AsParallel()
                .Do(b => b.Dispose())
                .Done();
        }

        private async Task ClearMeABus()
        {
            // Filter types we care about to only our own test's namespace. It's a performance optimisation because creating and
            // deleting queues and topics is slow.
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {"Some.Namespace.That.Does.Not.Exist"});

            var logger = new ConsoleLogger();

            var busBuilder = new BusBuilder().Configure()
                                             .WithNames("IntegrationTestHarness", Environment.MachineName)
                                             .WithConnectionString(CommonResources.ServiceBusConnectionString)
                                             .WithTypesFrom(typeProvider)
                                             .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                             .WithLogger(logger)
                                             .WithDebugOptions(
                                                 dc =>
                                                 dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                     "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                ;

            using (var bus = busBuilder.Build())
            {
                await bus.Start();
            }
        }

        private Task<Bus> BuildMeABus()
        {
            return Task.Run(async () =>
                                  {
                                      // Filter types we care about to only our own test's namespace. It's a performance optimisation because creating and
                                      // deleting queues and topics is slow.
                                      var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

                                      var logger = new ConsoleLogger();

                                      var bus = new BusBuilder().Configure()
                                                                .WithNames("IntegrationTestHarness", Environment.MachineName)
                                                                .WithConnectionString(CommonResources.ServiceBusConnectionString)
                                                                .WithTypesFrom(typeProvider)
                                                                .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                                                .WithLogger(logger)
                                                                .Build();
                                      bus.ShouldNotBe(null);
                                      await bus.Start();
                                      return bus;
                                  });
        }
    }
}