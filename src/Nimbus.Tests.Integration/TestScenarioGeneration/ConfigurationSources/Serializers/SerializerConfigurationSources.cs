using System.Collections;
using System.Collections.Generic;
using Nimbus.InfrastructureContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Serializers
{
    public class SerializerConfigurationSources : IEnumerable<IConfigurationScenario<ISerializer>>
    {
        private readonly ITypeProvider _typeProvider;

        public SerializerConfigurationSources(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public IEnumerator<IConfigurationScenario<ISerializer>> GetEnumerator()
        {
            yield return new Json();
            yield return new DataContract(_typeProvider);
            //yield return new ProtoBuf(); //FIXME do we still want this?
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}