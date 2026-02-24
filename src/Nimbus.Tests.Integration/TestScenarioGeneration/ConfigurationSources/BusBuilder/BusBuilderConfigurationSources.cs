using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Compressors;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.IoCContainers;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Routers;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Serializers;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.SynchronizationContexts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.BusBuilder
{
    public class BusBuilderConfigurationSources : IEnumerable<IConfigurationScenario<BusBuilderConfiguration>>
    {
        private readonly Type _testFixtureType;

        public BusBuilderConfigurationSources(Type testFixtureType)
        {
            _testFixtureType = testFixtureType;
        }

        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        public IEnumerator<IConfigurationScenario<BusBuilderConfiguration>> GetEnumerator()
        {
            var typeProvider = new TestHarnessTypeProvider(
                [_testFixtureType.Assembly],
                [_testFixtureType.Namespace]);
            var logger = TestHarnessLoggerFactory.Create(Guid.NewGuid(), GetType().FullName);

            // Default configurations (first of each)
            var defaultRouter = new RouterConfigurationSources().First();
            var defaultSerializer = new SerializerConfigurationSources(typeProvider).First();
            var defaultIoC = new IoCContainerConfigurationSources().First();
            var defaultCompressor = new CompressorScenariosSource().First();
            var defaultSyncContext = new SynchronizationContextConfigurationSources().First();

            // Generate scenarios based on selected transport
            foreach (var transport in GetSelectedTransports())
            {
                yield return new BusBuilderScenario(
                    typeProvider,
                    logger,
                    transport,
                    defaultRouter,
                    defaultSerializer,
                    defaultCompressor,
                    defaultIoC,
                    defaultSyncContext);
            }
        }

        private IEnumerable<IConfigurationScenario<TransportConfiguration>> GetSelectedTransports()
        {
            var allTransports = new TransportConfigurationSources().ToList();

            switch (TransportSelector.SelectedTransport)
            {
                case TestTransport.InProcess:
                    return allTransports.Where(t => t.Name == "InProcess");
                case TestTransport.Redis:
                    return allTransports.Where(t => t.Name == "Redis");
                case TestTransport.Amqp:
                    return allTransports.Where(t => t.Name == "Amqp");
                case TestTransport.AzureServiceBus:
                    return allTransports.Where(t => t.Name == "AzureServiceBus");
                case TestTransport.All:
                default:
                    return allTransports;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}