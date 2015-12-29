using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Serializers
{
    internal class DataContract : IConfigurationScenario<ISerializer>
    {
        private readonly ITypeProvider _typeProvider;

        public DataContract(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public string Name { get; } = "DataContract";
        public string[] Categories { get; } = {"DataContract"};

        public ScenarioInstance<ISerializer> CreateInstance()
        {
            var serializer = new DataContractSerializer(_typeProvider);
            var instance = new ScenarioInstance<ISerializer>(serializer);
            return instance;
        }
    }
}