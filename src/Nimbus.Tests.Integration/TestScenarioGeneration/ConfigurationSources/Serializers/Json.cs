using System.Collections.Generic;
using Nimbus.InfrastructureContracts;
using Nimbus.Serializers.Json;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Serializers
{
    internal class Json : ConfigurationScenario<ISerializer>
    {
        protected override IEnumerable<string> AdditionalCategories
        {
            get { yield return "SmokeTest"; }
        }

        public override ScenarioInstance<ISerializer> CreateInstance()
        {
            var serializer = new JsonSerializer();
            var instance = new ScenarioInstance<ISerializer>(serializer);
            return instance;
        }
    }
}