using System.Collections.Generic;
using Nimbus.Infrastructure.Compression;
using Nimbus.InfrastructureContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Compressors
{
    public class NullCompressorScenario : ConfigurationScenario<ICompressor>
    {
        protected override IEnumerable<string> AdditionalCategories
        {
            get { yield return "SmokeTest"; }
        }

        public override ScenarioInstance<ICompressor> CreateInstance()
        {
            var compressor = new NullCompressor();
            var instance = new ScenarioInstance<ICompressor>(compressor);
            return instance;
        }
    }
}