using Nimbus.Infrastructure.Serialization;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Serializers
{
    internal class DataContract : ConfigurationScenario<ISerializer>
    {
        private readonly ITypeProvider _typeProvider;

        public DataContract(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public override ScenarioInstance<ISerializer> CreateInstance()
        {
            var serializer = new DataContractSerializer(_typeProvider);
            var instance = new ScenarioInstance<ISerializer>(serializer);
            return instance;
        }
    }
}