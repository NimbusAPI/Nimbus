using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Configuration.Transport;
using Nimbus.Extensions;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Infrastructure.Routing;
using Nimbus.Serializers.Json;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.BusBuilderTests
{
    [TestFixture]
    [Timeout(TimeoutSeconds*1000)]
    public class WhenCreatingMultipleBusInstancesPointedAtTheSameEndpoint
    {
        protected const int TimeoutSeconds = 120;

        private Bus[] _buses;
        private readonly ILogger _logger = TestHarnessLoggerFactory.Create();
        private readonly string _globalPrefix = Guid.NewGuid().ToString();

        [Test]
        [TestCaseSource(typeof(AllTransportConfigurations))]
        public async Task NoneOfThemShouldGoBang(string testName, IConfigurationScenario<TransportConfiguration> scenario)
        {
            await ClearMeABus(scenario);

            _buses = await Enumerable.Range(0, 10)
                                     .Select(i => Task.Run(async () => await BuildMeABus(scenario)))
                                     .SelectResultsAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _buses?.AsParallel()
                   .Do(b => b.Dispose())
                   .Done();
        }

        private async Task ClearMeABus(IConfigurationScenario<TransportConfiguration> scenario)
        {
            using (var instance = scenario.CreateInstance())
            {
                // We want a namespace that doesn't exist here so that all the queues and topics are removed.
                var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {"Some.Namespace.That.Does.Not.Exist"});
                var transportConfiguration = instance.Configuration;

                var busBuilder = new BusBuilder().Configure()
                                                 .WithNames("MyTestSuite", Environment.MachineName)
                                                 .WithGlobalPrefix(_globalPrefix)
                                                 .WithTransport(transportConfiguration)
                                                 .WithRouter(new DestinationPerMessageTypeRouter())
                                                 .WithSerializer(new JsonSerializer())
                                                 .WithDeliveryRetryStrategy(new ImmediateRetryDeliveryStrategy())
                                                 .WithDependencyResolver(new DependencyResolver(typeProvider))
                                                 .WithTypesFrom(typeProvider)
                                                 .WithDefaultTimeout(TimeSpan.FromSeconds(TimeoutSeconds))
                                                 .WithHeartbeatInterval(TimeSpan.MaxValue)
                                                 .WithLogger(_logger)
                                                 .WithDebugOptions(
                                                     dc =>
                                                         dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                             "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                    ;

                using (var bus = busBuilder.Build())
                {
                    await bus.Start();
                    await bus.Stop();
                }
            }
        }

        private async Task<Bus> BuildMeABus(IConfigurationScenario<TransportConfiguration> scenario)
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            using (var instance = scenario.CreateInstance())
            {
                var transportConfiguration = instance.Configuration;

                var configuration = new BusBuilder().Configure()
                                                    .WithNames("MyTestSuite", Environment.MachineName)
                                                    .WithGlobalPrefix(_globalPrefix)
                                                    .WithTransport(transportConfiguration)
                                                    .WithRouter(new DestinationPerMessageTypeRouter())
                                                    .WithSerializer(new JsonSerializer())
                                                    .WithDeliveryRetryStrategy(new ImmediateRetryDeliveryStrategy())
                                                    .WithDependencyResolver(new DependencyResolver(typeProvider))
                                                    .WithTypesFrom(typeProvider)
                                                    .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                                    .WithHeartbeatInterval(TimeSpan.MaxValue)
                                                    .WithLogger(_logger)
                    ;

                var bus = configuration.Build();
                await bus.Start();
                await bus.Stop();
                return bus;
            }
        }
    }
}