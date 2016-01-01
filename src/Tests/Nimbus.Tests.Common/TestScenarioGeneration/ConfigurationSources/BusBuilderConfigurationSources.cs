using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nimbus.Configuration;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Routers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Serializers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
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
            var logger = TestHarnessLoggerFactory.Create();

            foreach (var transport in new TransportConfigurationSources())
            {
                foreach (var router in new RouterConfigurationSources())
                {
                    foreach (var serializer in new SerializerConfigurationSources(typeProvider))
                    {
                        foreach (var iocContainer in new IoCContainerConfigurationSources())
                        {
                            yield return new BusBuilderScenario(typeProvider, logger, transport, router, serializer, iocContainer);
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