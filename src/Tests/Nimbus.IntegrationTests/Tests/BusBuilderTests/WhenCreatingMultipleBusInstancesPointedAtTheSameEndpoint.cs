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
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.BusBuilderTests
{
    [TestFixture]
    [Timeout(120*1000)]
    public class WhenCreatingMultipleBusInstancesPointedAtTheSameEndpoint
    {
        private Bus[] _buses;
        private readonly ILogger _logger = TestHarnessLoggerFactory.Create();

        [Test]
        [TestCaseSource(typeof (AllTransportConfigurations))]
        public async Task NoneOfThemShouldGoBang(string testName, TransportConfiguration transportConfiguration)
        {
            await ClearMeABus(transportConfiguration);

            _buses = await Enumerable.Range(0, 10)
                                     .Select(i => BuildMeABus(transportConfiguration))
                                     .SelectResultsAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _buses?.AsParallel()
                   .Do(b => b.Dispose())
                   .Done();
        }

        private async Task ClearMeABus(TransportConfiguration transportConfiguration)
        {
            // We want a namespace that doesn't exist here so that all the queues and topics are removed.
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {"Some.Namespace.That.Does.Not.Exist"});

            var busBuilder = new BusBuilder().Configure()
                                             .WithTransport(transportConfiguration)
                                             .WithRouter(new DestinationPerMessageTypeRouter())
                                             .WithSerializer(new JsonSerializer())
                                             .WithDeliveryRetryStrategy(new ImmediateRetryDeliveryStrategy())
                                             .WithDependencyResolver(new DependencyResolver(typeProvider))
                                             .WithNames("MyTestSuite", Environment.MachineName)
                                             .WithTypesFrom(typeProvider)
                                             .WithDefaultTimeout(TimeSpan.FromSeconds(10))
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
            }
        }

        private Task<Bus> BuildMeABus(TransportConfiguration transportConfiguration)
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            return Task.Run(async () =>
                                  {
                                      // we grab a new one of these each time so that we're guaranteed to get a new instance of
                                      // all the builders etc. It's a bit ick. Sorry :(  -andrewh 21/12/2015
                                      var actualTransportConfiguration = new TransportConfigurationSources()
                                          .Where(tcs => tcs.Configuration.GetType() == transportConfiguration.GetType())
                                          .First();

                                      var configuration = new BusBuilder().Configure()
                                                                          .WithTransport(actualTransportConfiguration.Configuration)
                                                                          .WithRouter(new DestinationPerMessageTypeRouter())
                                                                          .WithSerializer(new JsonSerializer())
                                                                          .WithDeliveryRetryStrategy(new ImmediateRetryDeliveryStrategy())
                                                                          .WithDependencyResolver(new DependencyResolver(typeProvider))
                                                                          .WithNames("MyTestSuite", Environment.MachineName)
                                                                          .WithTypesFrom(typeProvider)
                                                                          .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                                                          .WithHeartbeatInterval(TimeSpan.MaxValue)
                                                                          .WithLogger(_logger)
                                          ;

                                      var bus = configuration.Build();
                                      await bus.Start();
                                      return bus;
                                  });
        }
    }
}