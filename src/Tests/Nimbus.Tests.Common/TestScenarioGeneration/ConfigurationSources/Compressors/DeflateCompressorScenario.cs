using Nimbus.Infrastructure.Compression;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Compressors
{
    public class DeflateCompressorScenario : ConfigurationScenario<ICompressor>
    {
        public override ScenarioInstance<ICompressor> CreateInstance()
        {
            var compressor = new DeflateCompressor();
            var instance = new ScenarioInstance<ICompressor>(compressor);
            return instance;
        }
    }
}