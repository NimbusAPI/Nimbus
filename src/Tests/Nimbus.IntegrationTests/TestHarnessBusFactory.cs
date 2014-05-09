using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Logger;

namespace Nimbus.IntegrationTests
{
    public class TestHarnessBusFactory
    {
        private readonly Type _testFixtureType;

        public TestHarnessBusFactory(Type testFixtureType)
        {
            _testFixtureType = testFixtureType;
        }

        public async Task<Bus> CreateAndStart()
        {
            var logger = new ConsoleLogger();

            // Filter types we care about to only our own test's namespace. It's a performance optimisation because creating and
            // deleting queues and topics is slow.
            var typeProvider = new TestHarnessTypeProvider(new[] {_testFixtureType.Assembly}, new[] {_testFixtureType.Namespace});

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ServiceBusConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithDependencyResolver(new DependencyResolver(typeProvider))
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithLogger(logger)
                                      .WithDebugOptions(
                                          dc =>
                                              dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                  "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            await bus.Start();

            return bus;
        }
    }
}