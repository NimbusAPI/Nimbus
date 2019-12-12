using Nimbus.Infrastructure.Serialization;
using Nimbus.InfrastructureContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Serializers
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