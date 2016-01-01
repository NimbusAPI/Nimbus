using System;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Serializers
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