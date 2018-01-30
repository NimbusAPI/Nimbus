using System.Collections.Generic;
using Nimbus.Infrastructure.Compression;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Compressors
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