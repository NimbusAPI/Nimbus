using System;
using Nimbus.InfrastructureContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Serializers
{
    internal class ProtoBuf : ConfigurationScenario<ISerializer>
    {
        public override ScenarioInstance<ISerializer> CreateInstance()
        {
            throw new NotImplementedException();
            //var serializer = new ProtoBufSerializer();
            //var instance = new ScenarioInstance<ISerializer>(serializer);
            //return instance;
        }
    }
}