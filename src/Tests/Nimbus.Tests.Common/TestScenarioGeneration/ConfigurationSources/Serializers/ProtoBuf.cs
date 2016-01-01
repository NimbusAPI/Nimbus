using System;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Serializers
{
    internal class ProtoBuf : IConfigurationScenario<ISerializer>
    {
        public string Name { get; } = "ProtoBuf";
        public string[] Categories { get; } = {"ProtoBuf"};

        public ScenarioInstance<ISerializer> CreateInstance()
        {
            throw new NotImplementedException();
            //var serializer = new ProtoBufSerializer();
            //var instance = new ScenarioInstance<ISerializer>(serializer);
            //return instance;
        }
    }
}