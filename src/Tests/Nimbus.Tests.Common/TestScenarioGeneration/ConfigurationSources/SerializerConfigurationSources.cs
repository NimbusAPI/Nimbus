using System.Collections;
using System.Collections.Generic;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Serializers.Json;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    public class SerializerConfigurationSources : IEnumerable<PartialConfigurationScenario<ISerializer>>
    {
        private readonly ITypeProvider _typeProvider;

        public SerializerConfigurationSources(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public IEnumerator<PartialConfigurationScenario<ISerializer>> GetEnumerator()
        {
            yield return new PartialConfigurationScenario<ISerializer>(
                nameof(DataContractSerializer),
                new DataContractSerializer(_typeProvider));

            yield return new PartialConfigurationScenario<ISerializer>(
                nameof(JsonSerializer),
                new JsonSerializer());

            //FIXME do we still want this?
            //yield return new PartialConfigurationScenario<ISerializer>(
            //    nameof(ProtoBufSerializer),
            //    new ProtoBufSerializer()
            //    );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}