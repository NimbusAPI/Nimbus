using Nimbus.Serializers.Json;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Serializers
{
    internal class Json : IConfigurationScenario<ISerializer>
    {
        public string Name { get; } = "Json";
        public string[] Categories { get; } = {"Json"};

        public ScenarioInstance<ISerializer> CreateInstance()
        {
            var serializer = new JsonSerializer();
            var instance = new ScenarioInstance<ISerializer>(serializer);
            return instance;
        }
    }
}