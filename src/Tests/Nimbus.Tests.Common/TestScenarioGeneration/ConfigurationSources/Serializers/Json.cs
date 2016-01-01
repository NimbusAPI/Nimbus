using System.Collections.Generic;
using Nimbus.Serializers.Json;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Serializers
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