using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Compressors;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Routers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Serializers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.SynchronizationContexts;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.BusBuilder
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
            var typeProvider = new TestHarnessTypeProvider(new[] {_testFixtureType.Assembly}, new[] {_testFixtureType.Namespace});
            var logger = TestHarnessLoggerFactory.Create(Guid.NewGuid(), GetType().FullName);

            foreach (var syncContext in new SynchronizationContextConfigurationSources())
            {
                // in-process transport with all other combinations
                foreach (var transport in new TransportConfigurationSources().Take(1))
                {
                    foreach (var router in new RouterConfigurationSources())
                    {
                        foreach (var serializer in new SerializerConfigurationSources(typeProvider))
                        {
                            foreach (var iocContainer in new IoCContainerConfigurationSources())
                            {
                                foreach (var compressor in new CompressorScenariosSource())
                                {
                                    yield return new BusBuilderScenario(typeProvider, logger, transport, router, serializer, compressor, iocContainer, syncContext);
                                }
                            }
                        }
                    }
                }

                // all transports with defaults for everything else
                foreach (var transport in new TransportConfigurationSources().Skip(1))
                {
                    foreach (var router in new RouterConfigurationSources().Take(1))
                    {
                        foreach (var serializer in new SerializerConfigurationSources(typeProvider).Take(1))
                        {
                            foreach (var iocContainer in new IoCContainerConfigurationSources().Take(1))
                            {
                                foreach (var compressor in new CompressorScenariosSource().Take(1))
                                {
                                    yield return new BusBuilderScenario(typeProvider, logger, transport, router, serializer, compressor, iocContainer, syncContext);
                                }
                            }
                        }
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