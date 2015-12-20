using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.IntegrationTests.Stubs;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.TestScenarioGeneration
{
    public class BusBuilderConfigurationSources : IEnumerable<PartialConfigurationScenario<BusBuilderConfiguration>>
    {
        private readonly Type _testFixtureType;

        public BusBuilderConfigurationSources(Type testFixtureType)
        {
            _testFixtureType = testFixtureType;
        }

        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        public IEnumerator<PartialConfigurationScenario<BusBuilderConfiguration>> GetEnumerator()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {_testFixtureType.Assembly}, new[] {_testFixtureType.Namespace});
            var logger = TestHarnessLoggerFactory.Create();

            foreach (var transport in new TransportConfigurationSources())
            {
                foreach (var router in new RouterConfigurationSources())
                {
                    foreach (var serializer in new SerializerConfigurationSources(typeProvider))
                    {
                        var scenarioName = PartialConfigurationScenario.Combine(transport.Name, router.Name, serializer.Name);

                        var configuration = new BusBuilder().Configure()
                                                            .WithTransport(transport.Configuration)
                                                            .WithRouter(router.Configuration)
                                                            .WithSerializer(serializer.Configuration)
                                                            .WithDeliveryRetryStrategy(new ImmediateRetryDeliveryStrategy())
                                                            .WithNames("MyTestSuite", Environment.MachineName)
                                                            .WithTypesFrom(typeProvider)
                                                            .WithGlobalInboundInterceptorTypes(
                                                                typeProvider.InterceptorTypes.Where(t => typeof (IInboundInterceptor).IsAssignableFrom(t)).ToArray())
                                                            .WithGlobalOutboundInterceptorTypes(
                                                                typeProvider.InterceptorTypes.Where(t => typeof (IOutboundInterceptor).IsAssignableFrom(t)).ToArray())
                                                            .WithDependencyResolver(new DependencyResolver(typeProvider))
                                                            .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                                            .WithHeartbeatInterval(TimeSpan.MaxValue)
                                                            .WithLogger(logger)
                                                            .WithDebugOptions(
                                                                dc =>
                                                                    dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                                        "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."));

                        yield return new PartialConfigurationScenario<BusBuilderConfiguration>(scenarioName, configuration);
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}